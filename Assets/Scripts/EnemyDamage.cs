using UnityEngine;

public class EnemyDamage : MonoBehaviour
{
    public int damageAmount = 1;

    private void OnTriggerEnter(Collider other)
    {
        // 1. 트리거 안으로 무언가 들어옴
        Debug.Log($"[Trigger] 무언가 감지됨: {other.name} (Tag: {other.tag})");

        if (other.CompareTag("Player"))
        {
            Debug.Log("[Trigger] 플레이어 태그 확인!");
            PlayerHealth player = other.GetComponent<PlayerHealth>();
            if (player != null)
            {
                player.TakeDamage(damageAmount);
            }
            else
            {
                Debug.LogWarning("[Trigger] PlayerHealth 스크립트를 찾을 수 없습니다.");
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 1. 물리적 충돌 발생
        Debug.Log($"[Collision] 물리 충돌 발생: {collision.gameObject.name}");

        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth player = collision.gameObject.GetComponent<PlayerHealth>();
            if (player != null)
            {
                player.TakeDamage(damageAmount);
            }
        }
    }
}