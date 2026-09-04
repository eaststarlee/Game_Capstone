using UnityEngine;

[RequireComponent(typeof(Collider))]
public class InkArea : MonoBehaviour
{
    [Header("Ink Type")]
    public InkType inkType = InkType.Default;

    [Header("Red: Ceiling Walk")]
    // 이전에 있던 speedMultiplier 제거

    [Header("Blue: Auto Super Jump")]
    public float superJumpForce = 12f;

    [Header("Yellow: Wall Stick")]
    public bool enableWallRun = false;
    public float wallRunUpSpeed = 0f;      // 자동상승 제거 (호환용 유지)
    public float wallRunGrav = -2.5f;      // 벽 부착 중 낙하 완화
    public float wallCheckDist = 0.6f;
    public LayerMask wallMask = ~0;

    [HideInInspector] public Vector3 surfaceNormal; // 투사체가 남겨둔 벽 방향

    private void Reset()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
            col.isTrigger = true;
    }

    private void Awake()
    {
        Collider col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
            col.isTrigger = true;
    }

    private void Start()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            if (col is BoxCollider box)
            {
                // 천장/벽면에 플레이어가 붙어 이동 시(Skin Width 등으로 인해)
                // OnTriggerStay 영역에서 벗어나 능력이 끊기는 걸 방지하기 위해 Z축 두께를 유저 방향으로 키움
                Vector3 size = box.size;
                size.z = Mathf.Max(size.z, 2.0f);
                box.size = size;

                Vector3 center = box.center;
                center.z = -0.5f; // 바깥(유저)을 향하는 방향으로 콜라이더 중심 이동
                box.center = center;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerController pc = other.GetComponentInParent<PlayerController>();
        if (pc == null) return;

        switch (inkType)
        {
            case InkType.Red:
                pc.RegisterRedInkContact(surfaceNormal);
                break;

            case InkType.Blue:
                pc.ForceJump(superJumpForce);
                break;

            case InkType.Yellow:
                // 바닥/천장 제외, 벽면일 때만
                if (Mathf.Abs(surfaceNormal.y) < 0.5f)
                {
                    pc.RegisterYellowInkContact(
                        surfaceNormal,
                        wallRunUpSpeed,
                        wallRunGrav,
                        wallCheckDist,
                        wallMask
                    );
                }
                break;

            case InkType.Black:
                break;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (inkType != InkType.Yellow && inkType != InkType.Red) return;

        PlayerController pc = other.GetComponentInParent<PlayerController>();
        if (pc == null) return;

        if (inkType == InkType.Yellow && Mathf.Abs(surfaceNormal.y) < 0.5f)
        {
            pc.RegisterYellowInkContact(
                surfaceNormal,
                wallRunUpSpeed,
                wallRunGrav,
                wallCheckDist,
                wallMask
            );
        }
        else if (inkType == InkType.Red)
        {
            pc.RegisterRedInkContact(surfaceNormal);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerController pc = other.GetComponentInParent<PlayerController>();
        if (pc == null) return;

        switch (inkType)
        {
            case InkType.Red:
                pc.NotifyRedInkExit();
                break;

            case InkType.Blue:
                pc.DisableSuperJump();
                break;

            case InkType.Yellow:
                // 즉시 꺼버리면 데칼 여러 개 사이에서 깜빡이며 덜거덕
                pc.NotifyYellowInkExit();
                break;

            case InkType.Black:
                break;
        }
    }
}