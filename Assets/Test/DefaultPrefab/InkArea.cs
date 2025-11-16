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

    [Header("Yellow: Wall Run")]
    public bool enableWallRun = false;
    public float wallRunUpSpeed = 4f;
    public float wallRunGrav = -3f;
    public float wallCheckDist = 0.6f;
    public LayerMask wallMask = ~0;

    private void Reset()
    {
        // InkArea 붙이면 자동으로 Trigger로 만들어줌
        Collider col = GetComponent<Collider>();
        if (col != null)
            col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        // 디버그용 로그
        Debug.Log($"[InkArea] Enter {inkType} with {other.name}", this);

        PlayerController pc = other.GetComponentInParent<PlayerController>();
        if (pc == null) return;

        switch (inkType)
        {
            case InkType.Red:
                // 빨강: 이속 증가
                pc.surfaceSpeedMultiplier = Mathf.Max(pc.surfaceSpeedMultiplier, speedMultiplier);
                break;

            case InkType.Blue:
                // 🔵 파랑: 잉크 위에 들어오자마자 점프키 없이 바로 슈퍼 점프
                pc.ForceJump(superJumpForce);
                break;

            case InkType.Yellow:
                // 노랑: 벽달리기
                pc.EnableWallRun(true, wallRunUpSpeed, wallRunGrav, wallCheckDist, wallMask);
                break;

            case InkType.Black:
                // 검정: 파괴는 Projectile에서 처리 → 여기선 아무 것도 안 해도 됨
                break;

            default:
                break;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log($"[InkArea] Exit {inkType} with {other.name}", this);

        PlayerController pc = other.GetComponentInParent<PlayerController>();
        if (pc == null) return;

        switch (inkType)
        {
            case InkType.Red:
                pc.surfaceSpeedMultiplier = 1f;
                break;

            case InkType.Blue:
                // 지금은 "한 번 튕기고 끝"이라 나갈 때 해줄 건 없음.
                // 나중에 쿨타임/상태 넣고 싶으면 여기서 초기화하면 됨.
                pc.DisableSuperJump();  // 있어도 되고 없어도 되는 안전용
                break;

            case InkType.Yellow:
                pc.EnableWallRun(false, 0f, 0f, 0f, 0);
                break;

            case InkType.Black:
                break;

            default:
                break;
        }
    }
}
