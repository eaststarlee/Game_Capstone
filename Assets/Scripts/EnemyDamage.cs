using UnityEngine;

[RequireComponent(typeof(Collider))]
public class EnemyDamage : MonoBehaviour
{
    public int damageAmount = 1;

    [Tooltip("같은 플레이어에게 연속 데미지 방지(초)")]
    public float hitCooldown = 0.5f;

    private float lastHitTime = -999f;

    private void Awake()
    {
        // 이 스크립트는 'DamageTrigger' 같은 트리거 콜라이더 오브젝트에 붙인다는 전제.
        // 따라서 여기서 Collider 설정을 강제로 바꾸지 않음.
        Collider col = GetComponent<Collider>();
        if (!col.isTrigger)
        {
            Debug.LogWarning("[EnemyDamage] 이 컴포넌트는 Trigger Collider에 붙이는 걸 권장합니다. (현재 isTrigger=false)");
        }

        // Trigger 이벤트 안정성을 위해 Rigidbody가 필요하면,
        // 루트(Drone)에 Rigidbody를 두는 게 정석임.
        // 여기(트리거 자식)에는 Rigidbody를 붙이지 않습니다.
    }

    private void OnTriggerEnter(Collider other)
    {
        TryDamage(other);
    }

    private void OnTriggerStay(Collider other)
    {
        TryDamage(other);
    }

    private void TryDamage(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (Time.time - lastHitTime < hitCooldown) return;
        lastHitTime = Time.time;

        PlayerHealth ph = other.GetComponent<PlayerHealth>();
        if (ph == null) ph = other.GetComponentInParent<PlayerHealth>();

        if (ph != null)
        {
            ph.TakeDamage(damageAmount);
        }
        else
        {
            Debug.LogWarning("[EnemyDamage] Player 태그는 맞는데 PlayerHealth를 찾지 못했습니다.");
        }
    }
}
