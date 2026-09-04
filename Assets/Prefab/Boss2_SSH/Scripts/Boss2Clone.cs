using UnityEngine;
using System.Collections;

public class Boss2Clone : MonoBehaviour
{
    private Transform player;
    private int health = 3;
    private bool isAttacking = false;
    private bool isRushing = false;

    private float orbitRadius;
    private float orbitSpeed;
    private float angleOffset;
    private float currentAngle;

    public GameObject projectilePrefab;
    public GameObject hitEffectPrefab; // [추가] 맞았을 때 터지는 이펙트 프리팹
    private float fireRate = 1.5f;

    public void Init(Transform playerTarget, float radius, float speed, float offset)
    {
        player = playerTarget;
        orbitRadius = radius;
        orbitSpeed = speed;
        angleOffset = offset;
        currentAngle = 0;

        StartCoroutine(AttackRoutine());
    }

    public void StartOrbit() => isAttacking = true;

    void Update()
    {
        if (player == null || isRushing) return;

        if (isAttacking)
        {
            currentAngle += orbitSpeed * Time.deltaTime;
            float totalAngle = (currentAngle + angleOffset) * Mathf.Deg2Rad;

            Vector3 nextPos = player.position + new Vector3(Mathf.Cos(totalAngle), 0, Mathf.Sin(totalAngle)) * orbitRadius;
            transform.position = Vector3.Lerp(transform.position, nextPos, Time.deltaTime * 5f);

            transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
        }
    }

    IEnumerator AttackRoutine()
    {
        while (health > 0)
        {
            if (isAttacking && !isRushing && projectilePrefab != null)
            {
                // 발사 위치 높이 보정 (1.1f)
                Vector3 firePos = transform.position + (Vector3.up * 1.1f) + (transform.forward * 1.0f);
                Instantiate(projectilePrefab, firePos, transform.rotation);
            }
            yield return new WaitForSeconds(fireRate);
        }
    }

    // [수정] BossHealth.cs의 HandleCollision 로직 이식
    private void OnTriggerEnter(Collider other)
    {
        InkProjectileController projectile = other.GetComponent<InkProjectileController>();

        // 1. 검은 잉크 총알인지 확인
        if (projectile != null && projectile.inkType == InkType.Black)
        {
            // 2. 히트 이펙트 생성
            if (hitEffectPrefab != null)
            {
                GameObject effect = Instantiate(hitEffectPrefab, other.transform.position, Quaternion.identity);
                Destroy(effect, 1.0f); // 1초 뒤 이펙트 삭제
            }

            // 3. 데미지 처리 및 총알 제거
            TakeDamage();
            Destroy(other.gameObject);
        }
    }

    public void TakeDamage()
    {
        health--;
        Debug.Log($"분신 피격! 남은 체력: {health}");

        if (health <= 0)
        {
            // 사라질 때도 이펙트를 주고 싶다면 여기에 추가
            Destroy(gameObject);
        }
    }

    public void FinalRush(Vector3 targetPos)
    {
        isRushing = true;
        isAttacking = false; // 공전 모드 종료를 명시적으로 선언
        StopAllCoroutines();
        StartCoroutine(RushRoutine(targetPos));
    }

    IEnumerator RushRoutine(Vector3 target)
    {
        float duration = 0.5f;
        float elapsed = 0f;
        Vector3 startPos = transform.position;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, target, elapsed / duration);
            yield return null;
        }
        Destroy(gameObject);
    }
}