using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossHealth2 : MonoBehaviour
{
    public enum BossState { Idle, Normal, Groggy, Defeated }

    [Header("상태 관리")]
    public BossState currentStatus = BossState.Idle;
    public bool isInvincible = true;

    [Header("체력 설정")]
    public float maxHP = 1000f;
    public float currentHP;
    [Range(1f, 20f)]
    public float groggyDamageMultiplier = 10f;

    [Header("UI 및 효과")]
    public BossUIController2 uiController;
    public GameObject hitEffectPrefab;
    public float effectDestroyTime = 1.0f;

    [Header("이벤트 알림")]
    public Action OnBossActive;
    public Action OnBossReset;
    public Action OnGroggyStart;
    public Action OnGroggyEnd;
    public Action OnDefeated;

    [Header("처치됨(Defeated) 연출 설정")]
    public GameObject defeatedExplosionPrefab; // 1. 폭발형 이펙트 프리팹
    public GameObject objectToActivate;        // 2. 활성화할 씬 내 오브젝트 (포탈 등)

    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private BossController2 bossController;

    private void Awake()
    {
        currentHP = maxHP;
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        bossController = GetComponent<BossController2>();

        SetStatus(BossState.Idle);
    }

    private void SetStatus(BossState newState)
    {
        if (currentStatus == newState) return;

        BossState oldState = currentStatus;
        currentStatus = newState;

        // 상태 전환 디버그 메시지
        Debug.Log($"<color=cyan>[BossState]</color> {oldState} ➔ <b>{newState}</b> (시간: {Time.time:F2}s)");

        isInvincible = (newState == BossState.Idle || newState == BossState.Defeated);
    }

    // --- 브릿지 코드에서 호출할 Public 함수들 ---
    public void ActivateBoss()
    {
        if (currentStatus != BossState.Idle) return;

        SetStatus(BossState.Normal);

        if (uiController != null) uiController.SetVisible(true);
        if (bossController != null) bossController.enabled = true;

        OnBossActive?.Invoke();
    }

    public void ResetBoss()
    {
        if (currentStatus == BossState.Defeated || currentStatus == BossState.Idle) return;

        SetStatus(BossState.Idle);
        currentHP = maxHP;

        // 위치 초기화
        transform.position = initialPosition;
        transform.rotation = initialRotation;

        if (uiController != null) uiController.SetVisible(false);
        if (bossController != null) bossController.enabled = false;

        CleanupBossElements();
        OnBossReset?.Invoke();
    }

    // --- [핵심] 검은 잉크 총알 인식 로직 ---
    private void OnTriggerEnter(Collider other)
    {
        // 1. 영역 트리거용 플레이어 체크 (혹시 브릿지를 안 썼을 때를 대비한 안전장치)
        if (other.gameObject.layer == 2)
        {
            ActivateBoss();
            return;
        }

        // 2. 검은 잉크 총알 체크 (원본 BossHealth 로직 복구)
        HandleCollision(other, false);
    }

    public void HandleCollision(Collider other, bool isWeakPoint)
    {
        // 무적 상태거나 이미 쓰러졌으면 무시
        if (isInvincible || currentStatus == BossState.Defeated) return;

        InkProjectileController projectile = other.GetComponent<InkProjectileController>();
        if (projectile != null && projectile.inkType == InkType.Black)
        {
            // 이펙트 생성
            if (hitEffectPrefab != null)
            {
                GameObject effect = Instantiate(hitEffectPrefab, other.transform.position, Quaternion.identity);
                Destroy(effect, effectDestroyTime);
            }

            // 데미지 처리 (정수형 damage를 float로 처리하도록 보완)
            ProcessHit(1f, isWeakPoint);

            // 총알 제거
            Destroy(other.gameObject);
        }
    }

    public void ProcessHit(float damage, bool isWeakPoint)
    {
        if (currentStatus == BossState.Defeated || isInvincible) return;

        float finalDamage = damage;

        // 그로기 시 데미지 증폭
        if (currentStatus == BossState.Groggy)
        {
            finalDamage = damage * groggyDamageMultiplier;
        }
        else if (isWeakPoint)
        {
            // 약점 명중 시 즉시 그로기 (상태 변화 로직 호출)
            EnterGroggyState(8.0f);
        }

        currentHP -= finalDamage;
        Debug.Log($"<color=red>[Hit]</color> Damage: {finalDamage}, Current HP: {currentHP}");

        if (currentHP <= 0) Shutdown();
    }

    public void EnterGroggyState(float duration)
    {
        if (currentStatus == BossState.Defeated) return;
        StartCoroutine(GroggyRoutine(duration));
    }

    private IEnumerator GroggyRoutine(float duration)
    {
        SetStatus(BossState.Groggy);
        OnGroggyStart?.Invoke();

        yield return new WaitForSeconds(duration);

        if (currentStatus != BossState.Defeated && currentStatus != BossState.Idle)
        {
            SetStatus(BossState.Normal);
            OnGroggyEnd?.Invoke();
        }
    }

    private void Shutdown()
    {
        if (currentStatus == BossState.Defeated) return;

        SetStatus(BossState.Defeated);
        CleanupBossElements();

        if (uiController != null) uiController.SetVisible(false);
        OnDefeated?.Invoke();
        // 1. 보스 위치에 폭발형 이펙트 생성
        
        if (defeatedExplosionPrefab != null)
        {
            // 보스의 중심점이나 발바닥 위치에 생성 (여기서는 보스 현재 위치)
            GameObject explosion = Instantiate(defeatedExplosionPrefab, transform.position, Quaternion.identity);

            // 이펙트가 무한히 재생되는 것을 방지하기 위해 5초 뒤 자동 삭제 (시간 조절 가능)
            Destroy(explosion, 1.0f);
        } 

        // 2. 특정 씬 내 오브젝트 활성화
        if (objectToActivate != null)
        {
            objectToActivate.SetActive(true);
        }

        gameObject.SetActive(false);
    }

    private void CleanupBossElements() 
    {
        StopAllCoroutines();
        if (bossController != null) bossController.ClearAllPatterns();
    }
}