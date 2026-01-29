using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerHealth : MonoBehaviour
{
    [Header("HP System")]
    public int maxHealth = 5;
    public int currentHealth;

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
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible) return;

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

    // PlayerHealth.cs 파일 내부에 추가
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // 나(플레이어)와 부딪힌 물체에 EnemyDamage가 있는지 확인
        EnemyDamage enemy = hit.gameObject.GetComponent<EnemyDamage>();

        if (enemy != null)
        {
            // 피격 처리
            TakeDamage(enemy.damageAmount);
        }
    }
}