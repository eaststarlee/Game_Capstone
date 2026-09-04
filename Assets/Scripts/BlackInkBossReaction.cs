using System.Collections;
using UnityEngine;

public class BlackInkBossReaction : MonoBehaviour
{
    [Header("Knockback Settings")]
    public float knockbackDistance = 1.0f;
    public float knockbackDuration = 0.12f;
    public float liftAmount = 0.08f;
    public float hitCooldown = 0.2f;

    private float lastHitTime = -999f;
    private Coroutine knockbackRoutine;

    // 자동으로 같은 오브젝트의 Pathchaser를 찾음
    private Behaviour pathchaser;

    private void Awake()
    {
        pathchaser = GetComponent("Pathchaser") as Behaviour;

        if (pathchaser == null)
        {
            Debug.LogWarning("[BlackInkBossReaction] Pathchaser를 찾지 못했습니다.");
        }
    }

    public void ReactToBlackInk(Vector3 hitPoint)
    {
        Debug.Log("[BlackInkBossReaction] 검은 잉크 반응 호출됨");

        if (Time.time - lastHitTime < hitCooldown) return;
        lastHitTime = Time.time;

        if (knockbackRoutine != null)
        {
            StopCoroutine(knockbackRoutine);
        }

        knockbackRoutine = StartCoroutine(KnockbackRoutine(hitPoint));
    }

    private IEnumerator KnockbackRoutine(Vector3 hitPoint)
    {
        SetPathchaserEnabled(false);

        Vector3 startPos = transform.position;

        Vector3 dir = transform.position - hitPoint;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.0001f)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                dir = transform.position - player.transform.position;
                dir.y = 0f;
            }
        }

        if (dir.sqrMagnitude < 0.0001f)
        {
            dir = -transform.forward;
            dir.y = 0f;
        }

        dir.Normalize();

        Vector3 targetPos = startPos + dir * knockbackDistance;
        targetPos.y += liftAmount;

        float elapsed = 0f;

        while (elapsed < knockbackDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / knockbackDuration);
            float curve = 1f - Mathf.Pow(1f - t, 2f);

            transform.position = Vector3.Lerp(startPos, targetPos, curve);
            yield return null;
        }

        transform.position = targetPos;

        SetPathchaserEnabled(true);
        knockbackRoutine = null;
    }

    private void SetPathchaserEnabled(bool enabledState)
    {
        if (pathchaser != null)
        {
            pathchaser.enabled = enabledState;
        }
    }
}