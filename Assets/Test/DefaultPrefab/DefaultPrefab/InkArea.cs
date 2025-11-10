using UnityEngine;

public class InkArea : MonoBehaviour
{
    [Tooltip("잉크 위에 있을 때 이동속도 배율")]
    public float speedMultiplier = 1.5f;

    private void OnTriggerEnter(Collider other)
    {
        Apply(other, speedMultiplier);
    }

    private void OnTriggerStay(Collider other)
    {
        // 센서가 계속 겹쳐 있으면 매 프레임 보정
        Apply(other, speedMultiplier);
    }

    private void OnTriggerExit(Collider other)
    {
        Apply(other, 1f);
    }

    private void Apply(Collider other, float m)
    {
        // 플레이어의 자식(센서)로 들어와도 부모에서 PlayerController를 찾는다
        var pc = other.GetComponentInParent<PlayerController>();
        if (pc != null)
        {
            pc.surfaceSpeedMultiplier = m;
        }
    }
}
