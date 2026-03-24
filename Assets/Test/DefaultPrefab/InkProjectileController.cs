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

    [Header("Yellow Spread Settings")]
    [SerializeField] private float yellowMajorGap = 0.6f;          // 옆(가로) 간격
    [SerializeField] private float yellowMinorGap = 0.45f;         // 위아래(세로) 간격
    [SerializeField] private int yellowMajorMaxSteps = 18;         // 최대 가로 확산 단계
    [SerializeField] private int yellowMinorRows = 2;              // 위/아래 행 수 (총 2*rows+1)
    [SerializeField] private float yellowSpawnDelay = 0.005f;      // 퍼지는 연출 속도
    [SerializeField] private float yellowSurfaceProbeStart = 0.25f;
    [SerializeField] private float yellowSurfaceProbeDistance = 0.8f;
    [SerializeField] private float yellowNormalToleranceDot = 0.8f; // 표면 각도 변화 허용치

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

        // Black 잉크: 파괴 효과
        if (inkType == InkType.Black)
        {
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
            // 레이어가 "Wall"이거나 태그가 "Wall"인 경우 모두 포함
            bool isWall = (hitCol.gameObject.layer == LayerMask.NameToLayer("Wall")) || hitCol.CompareTag("Wall");

            if (inkType == InkType.Blue && isWall)
            {
                // Wall(벽)에는 파란 잉크가 칠해지지 않고, 바로 아래의 투사체 파괴 로직으로 넘어감
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

        // 투사체 숨김 및 파괴
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

        // 가로축: 표면 위 투사체 진행방향 기준 (시각 확산용)
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

        // 세로축: 표면 위 월드업 성분
        Vector3 minorAxis = Vector3.ProjectOnPlane(Vector3.up, surfaceNormal).normalized;
        if (minorAxis.sqrMagnitude < 0.0001f)
        {
            minorAxis = Vector3.Cross(majorAxis, surfaceNormal).normalized;
        }

        if (Mathf.Abs(Vector3.Dot(majorAxis, minorAxis)) > 0.95f)
        {
            minorAxis = Vector3.Cross(majorAxis, surfaceNormal).normalized;
        }

        // 중앙 먼저
        SpawnDecal(hitPoint, surfaceNormal, dir);

        // 가로 길게, 세로 얕게 확산 (시각용)
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

        // 중요: 트리거 무시 (기존 노란 잉크 데칼 다시 맞아서 꼬이는 것 방지)
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
                    if (area.speedMultiplier < 1.5f) area.speedMultiplier = 1.5f;
                    break;

                case InkType.Blue:
                    if (area.superJumpForce < 12f) area.superJumpForce = 12f;
                    break;

                case InkType.Yellow:
                    // 노란 잉크는 "착붙"만 담당. 자동 상승/자동 방향 강제 X
                    area.enableWallRun = true;
                    area.wallRunUpSpeed = 0f; // 자동 상승 제거
                    area.wallRunGrav = -2.5f; // 천천히 미끄러지게
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