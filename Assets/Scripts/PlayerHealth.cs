using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerHealth : MonoBehaviour
{
    [Header("HP System")]
    public int maxHealth = 5;
    public int currentHealth;

    // 자동 회복 관련 설정 변수
    [Header("Auto Regen System")]
    public float noDamageDuration = 7f; // 피격 후 대기 시간 (7초)
    public float regenSpeed = 2f;       // 한 칸 차는 시간 (2초)
    private float lastDamageTime;       // 마지막 피격 시점 기록
    private float currentRegenTimer;    // 회복 진행도 타이머

    [Header("Invincibility Settings")]
    public float invincibilityDuration = 3f;
    private bool isInvincible = false;

    [Header("Respawn Settings")]
    public Vector3 respawnPoint;
    public float deathYLevel = -100f;

    [Header("1인칭 시각 피드백 (무기 모델)")]
    public GameObject weaponRoot;
    private List<Renderer> weaponRenderers = new List<Renderer>();

    public static PlayerHealth Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;

        respawnPoint = transform.position;
        currentHealth = maxHealth;

        // 시작할 때 마지막 피격 시간 초기화
        lastDamageTime = Time.time;

        // 하위 모든 렌더러 자동 탐색
        if (weaponRoot != null)
        {
            Renderer[] renderers = weaponRoot.GetComponentsInChildren<Renderer>();
            weaponRenderers.AddRange(renderers);
            Debug.Log($"[PlayerHealth] {weaponRenderers.Count}개의 무기 렌더러를 찾았습니다.");
        }

        Debug.Log($"[PlayerHealth] 시스템 초기화. HP: {currentHealth}, 리스폰 위치: {respawnPoint}");
    }

    void Update()
    {
        if (transform.position.y < deathYLevel)
        {
            Debug.Log($"[PlayerHealth] 낙사로 인한 게임 오버! Y: {transform.position.y}");
            Respawn();
        }

        // 매 프레임 자동 회복 체크
        HandleAutoRegen();
    }

    // 자동 회복 로직 함수
    private void HandleAutoRegen()
    {
        // 체력이 이미 가득 찼으면 로직 중단
        if (currentHealth >= maxHealth)
        {
            currentRegenTimer = 0f;
            return;
        }

        // 마지막 피격 이후 7초가 지났는지 확인
        if (Time.time - lastDamageTime >= noDamageDuration)
        {
            // 2초 동안 타이머 누적
            currentRegenTimer += Time.deltaTime;

            // 타이머가 2초(regenSpeed)를 넘기면 체력 1 회복
            if (currentRegenTimer >= regenSpeed)
            {
                currentHealth++;
                currentRegenTimer = 0f; // 다음 칸을 위해 타이머 리셋
                Debug.Log($"[AutoRegen] 체력 자동 회복! 현재 HP: {currentHealth}");
            }
        }
        else
        {
            // 아직 7초가 안 지났으면 회복 진행도 0으로 유지
            currentRegenTimer = 0f;
        }
    }

    // UI 스크립트에서 현재 차오르는 비율(0.0 ~ 1.0)을 가져가기 위한 함수
    public float GetRegenProgress()
    {
        if (currentHealth >= maxHealth) return 0f;
        if (Time.time - lastDamageTime < noDamageDuration) return 0f;

        // 현재 진행도 / 목표 시간 (예: 1초 지났으면 0.5 반환)
        return Mathf.Clamp01(currentRegenTimer / regenSpeed);
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible) return;

        // 피격 시점 기록 및 회복 타이머 초기화
        lastDamageTime = Time.time;
        currentRegenTimer = 0f;

        currentHealth -= damage;
        Debug.Log($"[PlayerHealth] 피격! 남은 HP: {currentHealth}");

        if (currentHealth <= 0)
        {
            Debug.Log("[PlayerHealth] 체력 소진으로 인한 게임 오버!");
            Respawn();
        }
        else
        {
            StartCoroutine(BecomeInvincible());
        }
    }
    private IEnumerator BecomeInvincible()
    {
        isInvincible = true;

        float elapsed = 0f;
        while (elapsed < invincibilityDuration)
        {
            SetWeaponVisible(false);
            yield return new WaitForSeconds(0.15f);

            SetWeaponVisible(true);
            yield return new WaitForSeconds(0.15f);

            elapsed += 0.3f;
        }

        SetWeaponVisible(true);
        isInvincible = false;
        Debug.Log("[PlayerHealth] 무적 상태 해제");
    }

    private void SetWeaponVisible(bool visible)
    {
        foreach (Renderer rend in weaponRenderers)
        {
            if (rend != null) rend.enabled = visible;
        }
    }

    public void Respawn()
    {
        Debug.Log($"[PlayerHealth] 게임 오버: 부활 로직 실행. 위치: {respawnPoint}");

        StopAllCoroutines();
        currentHealth = maxHealth;
        isInvincible = false;
        SetWeaponVisible(true);

        // 부활 시에도 회복 타이머 초기화
        lastDamageTime = Time.time;
        currentRegenTimer = 0f;

        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false;
            transform.position = respawnPoint;
            cc.enabled = true;
        }
        else
        {
            transform.position = respawnPoint;
        }
    }
    public void SetRespawnPoint(Vector3 newPoint)
    {
        respawnPoint = newPoint;
        Debug.Log("[PlayerHealth] 세이브포인트 업데이트: " + newPoint);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        EnemyDamage enemy = hit.gameObject.GetComponent<EnemyDamage>();

        if (enemy != null)
        {
            TakeDamage(enemy.damageAmount);
        }
    }
}