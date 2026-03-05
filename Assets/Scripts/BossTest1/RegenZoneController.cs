using UnityEngine;

public class RegenZoneController : MonoBehaviour
{
    [Tooltip("체크하면 이 구역에 들어올 때 회복을 끕니다. 체크 해제하면 이 구역에서만 회복이 켜집니다.")]
    public bool disableRegenInThisZone = true;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                // 구역 진입 시 회복 설정 변경
                playerHealth.canRegen = !disableRegenInThisZone;
                Debug.Log($"[Zone] 플레이어 진입: 자동 회복 {(playerHealth.canRegen ? "활성화" : "비활성화")}");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                // 구역을 나갈 때 원래 상태(활성)로 복구
                playerHealth.canRegen = disableRegenInThisZone;
                Debug.Log($"[Zone] 플레이어 퇴장: 자동 회복 {(playerHealth.canRegen ? "활성화" : "비활성화")}");
            }
        }
    }
}