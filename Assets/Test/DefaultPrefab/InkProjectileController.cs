using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class InkProjectileController : MonoBehaviour
{
    [Header("Decal / Ink Settings")]
    [SerializeField] private GameObject inkDecalObjectPrefab;
    [SerializeField] private float lifeTime = 5f;
    [SerializeField] private Color prefabColor = Color.white;
    [SerializeField] public InkType inkType = InkType.Default;
    [SerializeField] private Vector3 decalScaleMultiplier = Vector3.one;

    [Header("Yellow Spread Settings")]
    [SerializeField] private float yellowMajorGap = 0.6f;
    [SerializeField] private float yellowMinorGap = 0.45f;
    [SerializeField] private int yellowMajorMaxSteps = 18;
    [SerializeField] private int yellowMinorRows = 2;
    [SerializeField] private float yellowSpawnDelay = 0.005f;
    [SerializeField] private float yellowSurfaceProbeStart = 0.25f;
    [SerializeField] private float yellowSurfaceProbeDistance = 0.8f;
    [SerializeField] private float yellowNormalToleranceDot = 0.8f;

    private Rigidbody rb;
    private Vector3 dir;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = new Material(renderer.material);
            renderer.material.color = prefabColor;
        }
    }

    private void Update()
    {
        if (rb != null)
        {
#if UNITY_6000_0_OR_NEWER
            dir = rb.linearVelocity;
#else
            dir = rb.velocity;
#endif
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Collider hitCol = collision.collider;

        // Black 잉크: 특수몹이면 넉백만, 아니면 기존 파괴
        if (inkType == InkType.Black)
        {
            BlackInkBossReaction specialReaction = hitCol.GetComponentInParent<BlackInkBossReaction>();

            if (specialReaction != null)
            {
                ContactPoint cp = collision.GetContact(0);
                specialReaction.ReactToBlackInk(cp.point);

                Destroy(gameObject);
                return;
            }

            if (hitCol.CompareTag("Breakable"))
            {
                IBreakable breakableByInterface = hitCol.GetComponentInParent<IBreakable>();
                if (breakableByInterface != null) breakableByInterface.Break();
                else
                {
                    if (hitCol.attachedRigidbody != null) Destroy(hitCol.attachedRigidbody.gameObject);
                    else Destroy(hitCol.gameObject);
                }
            }
            else
            {
                IBreakable breakable = hitCol.GetComponentInParent<IBreakable>();
                if (breakable != null) breakable.Break();
            }

            Destroy(gameObject);
            return;
        }

        // 데칼 / 잉크 영역 생성
        if (inkDecalObjectPrefab != null)
        {
            bool isWall = (hitCol.gameObject.layer == LayerMask.NameToLayer("Wall")) || hitCol.CompareTag("Wall");
            bool isCeiling = hitCol.CompareTag("Ceiling") || hitCol.CompareTag("ceiling");

            if (isCeiling && inkType != InkType.Red)
            {
                // 천장에는 오직 빨간색 잉크만 묻을 수 있음
            }
            else if (inkType == InkType.Blue && isWall)
            {
            }
            else if (inkType == InkType.Red && !isCeiling)
            {
                // 빨간 잉크는 오직 Ceiling 태그(또는 ceiling)가 붙어있는 오브젝트에만 묻어나옴
            }
            else if (inkType == InkType.Yellow)
            {
                StartCoroutine(SpreadYellowInkRoutine(collision));
            }
            else
            {
                ContactPoint cp = collision.GetContact(0);
                SpawnDecal(cp.point, cp.normal, dir);
            }
        }

        Collider myCol = GetComponent<Collider>();
        Renderer myRenderer = GetComponent<Renderer>();
        if (myCol != null) myCol.enabled = false;
        if (myRenderer != null) myRenderer.enabled = false;

        Destroy(gameObject, 2f);
    }

    private IEnumerator SpreadYellowInkRoutine(Collision collision)
    {
        ContactPoint cp = collision.GetContact(0);
        Vector3 hitPoint = cp.point;
        Vector3 surfaceNormal = cp.normal.normalized;
        Collider targetCollider = collision.collider;

        Vector3 majorAxis = Vector3.ProjectOnPlane(
            dir.sqrMagnitude > 0.0001f ? dir.normalized : transform.forward,
            surfaceNormal
        ).normalized;

        if (majorAxis.sqrMagnitude < 0.0001f)
        {
            majorAxis = Vector3.Cross(surfaceNormal, Vector3.up);
            if (majorAxis.sqrMagnitude < 0.0001f)
                majorAxis = Vector3.Cross(surfaceNormal, Vector3.right);
            majorAxis.Normalize();
        }

        Vector3 minorAxis = Vector3.ProjectOnPlane(Vector3.up, surfaceNormal).normalized;
        if (minorAxis.sqrMagnitude < 0.0001f)
        {
            minorAxis = Vector3.Cross(majorAxis, surfaceNormal).normalized;
        }

        if (Mathf.Abs(Vector3.Dot(majorAxis, minorAxis)) > 0.95f)
        {
            minorAxis = Vector3.Cross(majorAxis, surfaceNormal).normalized;
        }

        SpawnDecal(hitPoint, surfaceNormal, dir);

        for (int row = -yellowMinorRows; row <= yellowMinorRows; row++)
        {
            bool isCenterRow = (row == 0);

            Vector3 rowOffset = minorAxis * (row * yellowMinorGap);
            Vector3 rowCenterProbe = hitPoint + rowOffset;

            if (!TryProjectToSameSurface(rowCenterProbe, surfaceNormal, targetCollider, out RaycastHit rowHit))
                continue;

            if (!isCenterRow)
            {
                SpawnDecal(rowHit.point, rowHit.normal, dir);
                if (yellowSpawnDelay > 0f) yield return new WaitForSeconds(yellowSpawnDelay);
            }

            for (int side = -1; side <= 1; side += 2)
            {
                for (int step = 1; step <= yellowMajorMaxSteps; step++)
                {
                    Vector3 probe = rowHit.point + majorAxis * (side * step * yellowMajorGap);

                    if (!TryProjectToSameSurface(probe, surfaceNormal, targetCollider, out RaycastHit hit))
                        break;

                    if (Vector3.Dot(hit.normal.normalized, surfaceNormal) < yellowNormalToleranceDot)
                        break;

                    SpawnDecal(hit.point, hit.normal, dir);
                    if (yellowSpawnDelay > 0f) yield return new WaitForSeconds(yellowSpawnDelay);
                }
            }
        }
    }

    private bool TryProjectToSameSurface(Vector3 approxSurfacePoint, Vector3 surfaceNormal, Collider expectedCollider, out RaycastHit hit)
    {
        Vector3 rayStart = approxSurfacePoint + surfaceNormal * yellowSurfaceProbeStart;
        Vector3 rayDir = -surfaceNormal;

        if (Physics.Raycast(rayStart, rayDir, out hit, yellowSurfaceProbeDistance, ~0, QueryTriggerInteraction.Ignore))
        {
            bool sameCollider = hit.collider == expectedCollider;
            bool sameRoot = hit.collider.transform.root == expectedCollider.transform.root;

            if (sameCollider || sameRoot)
                return true;
        }

        return false;
    }

    private void SpawnDecal(Vector3 pos, Vector3 normal, Vector3 velocityDir)
    {
        GameObject inst = Instantiate(inkDecalObjectPrefab);

        // 데칼 크기 적용
        inst.transform.localScale = Vector3.Scale(inst.transform.localScale, decalScaleMultiplier);

        Setter setter = new Setter();
        setter.AlignDecalToSurface(inst, pos, normal, velocityDir);

        InkArea area = inst.GetComponent<InkArea>();
        if (area != null)
        {
            area.inkType = inkType;
            area.surfaceNormal = normal;

            switch (inkType)
            {
                case InkType.Red:
                    // Ceiling walk 상태로만 쓰이므로 추가 속성 설정 없음
                    break;

                case InkType.Blue:
                    if (area.superJumpForce < 12f) area.superJumpForce = 12f;
                    break;

                case InkType.Yellow:
                    area.enableWallRun = true;
                    area.wallRunUpSpeed = 0f;
                    area.wallRunGrav = -2.5f;
                    break;
            }
        }

        Destroy(inst, lifeTime);
    }

    public InkType GetInkType() { return inkType; }
}

public interface IBreakable
{
    void Break();
}