using UnityEngine;

[RequireComponent(typeof(Collider))]
public class InkArea : MonoBehaviour
{
    [Header("Ink Type")]
    public InkType inkType = InkType.Default;

    [Header("Red: Speed Up")]
    public float speedMultiplier = 1f;

    [Header("Blue: Auto Super Jump")]
    public float superJumpForce = 8f;

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

    private void OnTriggerEnter(Collider other)
    {
        PlayerController pc = other.GetComponentInParent<PlayerController>();
        if (pc == null) return;

        switch (inkType)
        {
            case InkType.Red:
                pc.surfaceSpeedMultiplier = Mathf.Max(pc.surfaceSpeedMultiplier, speedMultiplier);
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
        if (inkType != InkType.Yellow) return;

        PlayerController pc = other.GetComponentInParent<PlayerController>();
        if (pc == null) return;

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
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerController pc = other.GetComponentInParent<PlayerController>();
        if (pc == null) return;

        switch (inkType)
        {
            case InkType.Red:
                pc.surfaceSpeedMultiplier = 1f;
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