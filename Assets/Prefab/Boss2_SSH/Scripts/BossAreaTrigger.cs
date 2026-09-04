using UnityEngine;

public class BossAreaTrigger : MonoBehaviour
{
    [Header("연결할 보스의 Health 스크립트")]
    public BossHealth2 targetBossHealth;

    private void OnTriggerEnter(Collider other)
    {
        // 레이어 2번(Player) 진입 확인
        if (other.gameObject.layer == 2 && targetBossHealth != null)
        {
            Debug.Log($"<color=lime>[Area Trigger]</color> 플레이어 진입 감지 (Object: {other.name})");
            targetBossHealth.ActivateBoss();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // 레이어 2번(Player) 이탈 확인
        if (other.gameObject.layer == 2 && targetBossHealth != null)
        {
            Debug.Log($"<color=orange>[Area Trigger]</color> 플레이어 이탈 감지 (Object: {other.name})");
            targetBossHealth.ResetBoss();
        }
    }
}