using UnityEngine;
using UnityEngine.Rendering.Universal;

public class InkProjectileController : MonoBehaviour
{
    [Header("Decal / Ink Settings")]
    [SerializeField] private GameObject inkDecalObjectPrefab; // Red/Blue/Yellow/Black 데칼 프리팹
    [SerializeField] private float lifeTime = 5f;
    [SerializeField] private Color prefabColor = Color.white; // 탄환 메쉬 색칠
    [SerializeField] public InkType inkType = InkType.Default;

    private Rigidbody rb;
    private Vector3 dir;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        // 탄환 메쉬 색칠 (있을 때만)
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
            dir = rb.linearVelocity;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Collider hitCol = collision.collider;

        // === Black 잉크: 파괴 효과 ===
        if (inkType == InkType.Black)
        {
            // 1) Tag 기반 파괴
            if (hitCol.CompareTag("Breakable"))
            {
                IBreakable breakableByInterface = hitCol.GetComponentInParent<IBreakable>();
                if (breakableByInterface != null)
                {
                    breakableByInterface.Break();
                }
                else
                {
                    if (hitCol.attachedRigidbody != null)
                        Destroy(hitCol.attachedRigidbody.gameObject);
                    else
                        Destroy(hitCol.gameObject);
                }
            }
            else
            {
                // 2) Tag 없이 IBreakable만 붙어있는 케이스 지원
                IBreakable breakable = hitCol.GetComponentInParent<IBreakable>();
                if (breakable != null)
                {
                    breakable.Break();
                }
            }

            // 🔥 검정 잉크는 파괴 로직 실행 후 데칼을 생성하지 않고 바로 종료
            Destroy(gameObject);
            return;
        }

        // === 데칼 / 잉크 영역 생성 (Red, Blue, Yellow 등) ===
        if (inkDecalObjectPrefab != null)
        {
            GameObject inst = Instantiate(inkDecalObjectPrefab);

            ContactPoint cp = collision.GetContact(0);

            Setter setter = new Setter();
            setter.AlignDecalToSurface(inst, cp.point, cp.normal, dir);

            InkArea area = inst.GetComponent<InkArea>();
            if (area != null)
            {
                area.inkType = inkType;

                switch (inkType)
                {
                    case InkType.Red:
                        if (area.speedMultiplier < 1.5f)
                            area.speedMultiplier = 1.5f;
                        break;

                    case InkType.Blue:
                        if (area.superJumpForce < 8f)
                            area.superJumpForce = 8f;
                        break;

                    case InkType.Yellow:
                        area.enableWallRun = true;
                        if (area.wallRunUpSpeed < 4f)
                            area.wallRunUpSpeed = 4f;
                        if (area.wallRunGrav > -3f)
                            area.wallRunGrav = -3f;
                        break;

                    default:
                        break;
                }
            }

            Destroy(inst, lifeTime);
        }

        Destroy(gameObject);
    }
    public InkType GetInkType() { return inkType; }
}

// 검은 잉크가 호출하는 인터페이스
public interface IBreakable
{
    void Break();
}

