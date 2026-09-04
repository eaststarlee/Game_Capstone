using UnityEngine;

[RequireComponent(typeof(Collider))]
public class EnemyInstantKill : MonoBehaviour
{
    [Tooltip("같은 플레이어에게 연속 즉사 처리 방지(초)")]
    public float hitCooldown = 0.5f;

    private float lastHitTime = -999f;

    private void Awake()
    {
        Collider col = GetComponent<Collider>();
        if (!col.isTrigger)
        {
            Debug.LogWarning("[EnemyInstantKill] 이 컴포넌트는 Trigger Collider에 붙이는 걸 권장합니다. (현재 isTrigger=false)");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        TryKill(other);
    }

    private void OnTriggerStay(Collider other)
    {
        TryKill(other);
    }

    private void TryKill(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (Time.time - lastHitTime < hitCooldown) return;
        lastHitTime = Time.time;

        Rigidbody playerRb = other.attachedRigidbody;
        if (playerRb != null && playerRb.IsSleeping())
        {
            playerRb.WakeUp();
        }

        PlayerHealth ph = other.GetComponent<PlayerHealth>();
        if (ph == null) ph = other.GetComponentInParent<PlayerHealth>();

        if (ph != null)
        {
            // 현재 남은 체력을 한 번에 전부 소진
            ph.TakeDamage(ph.currentHealth);
        }
        else
        {
            Debug.LogWarning("[EnemyInstantKill] Player 태그는 맞는데 PlayerHealth를 찾지 못했습니다.");
        }
    }
}