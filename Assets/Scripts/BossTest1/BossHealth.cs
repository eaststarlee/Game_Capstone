using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossHealth : MonoBehaviour
{
    public enum BossState { Normal, Groggy, Defeated }

    [Header("체력 설정")]
    public int maxHP = 500;
    public int currentHP;
    public BossState currentStatus = BossState.Normal;

    [Header("UI 참조")]
    public BossUIController uiController; // 인스펙터에서 직접 드래그!

    [Header("타격 이펙트 설정")]
    public GameObject hitEffectPrefab; // 프리팹(이펙트)을 인스펙터에서 연결하세요.
    public float effectDestroyTime = 1.0f; // 이펙트가 사라질 시간

    [Header("상태 이벤트")]
    public Action OnGroggyStart;
    public Action OnGroggyEnd;

    [Header("클리어 시 비활성화할 오브젝트들")]
    // 인스펙터에서 보스방 기믹(게이지, 함정 등)을 리스트에 넣으세요.
    public List<GameObject> objectsToDisableOnDefeat = new List<GameObject>();

    private CharacterController playerController;

    private void Awake()
    {
        currentHP = maxHP;

        // 플레이어 참조 미리 가져오기
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerController = player.GetComponent<CharacterController>();
        }
    }

    // 몸체(부모)에 맞았을 때 호출
    private void OnTriggerEnter(Collider other)
    {
        HandleCollision(other, false);
    }

    // 약점이나 몸체 어디서든 공통으로 처리할 로직
    public void HandleCollision(Collider other, bool isWeakPoint)
    {
        InkProjectileController projectile = other.GetComponent<InkProjectileController>();
        if (projectile != null && projectile.inkType == InkType.Black)
        {
            // 1. 충돌 지점 계산
            // Trigger의 경우 정확한 Contact Point를 제공하지 않으므로, 
            // 총알의 현재 위치(other.transform.position)를 타격 지점으로 사용합니다.
            Vector3 hitPoint = other.transform.position;

            // 2. 이펙트 생성
            if (hitEffectPrefab != null)
            {
                // 이펙트를 생성하고, 1초 뒤에 자동으로 삭제함
                GameObject effect = Instantiate(hitEffectPrefab, hitPoint, Quaternion.identity);
                Destroy(effect, effectDestroyTime);
            }

            // 3. 기존 데미지 로직 실행
            ProcessHit(1, isWeakPoint);
            Destroy(other.gameObject);
        }


    }

    public void ProcessHit(int damage, bool isWeakPoint)
    {
        if (currentStatus == BossState.Defeated) return;

        int finalDamage = damage;

        if (currentStatus == BossState.Groggy)
        {
            finalDamage = damage * 10;
        }
        else if (isWeakPoint)
        {
            // 일반 상태에서 약점 타격 시 그로기
            StopAllCoroutines();
            StartCoroutine(EnterGroggyState());
        }

        currentHP -= finalDamage;
        Debug.Log($"[Boss] 피격! 약점:{isWeakPoint}, 데미지:{finalDamage}, HP:{currentHP}");

        if (currentHP <= 0) Shutdown();
    }

    private IEnumerator EnterGroggyState()
    {
        currentStatus = BossState.Groggy;
        OnGroggyStart?.Invoke();
        yield return new WaitForSeconds(10f);
        currentStatus = BossState.Normal;
        OnGroggyEnd?.Invoke();
    }

    private void Shutdown()
    {
        if (currentStatus == BossState.Defeated) return;

        currentStatus = BossState.Defeated;
        Debug.Log("<color=green>보스 무력화 완료 (Defeated)</color>");

        if (uiController != null) uiController.SetVisible(false);

        // [핵심 추가] 플레이어 조작권 강제 복구
        if (playerController != null)
        {
            playerController.enabled = true;
            Debug.Log("<color=yellow>플레이어 조작권이 강제로 복구되었습니다.</color>");
        }

        // 1. 보스방의 모든 기믹 리스트 비활성화
        foreach (GameObject obj in objectsToDisableOnDefeat)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }

        // 2. 보스 본체 처리 (애니메이션이 있다면 애니메이션 후 끄는 것이 좋습니다)
        // 여기서는 일단 본체를 끕니다.
        gameObject.SetActive(false);
    }

    // 보스에게서 멀어진 경우 상태 초기화
    public void ResetHP()
    {
        // 이미 무력화되었거나 이미 체력이 가득 차 있다면 실행하지 않음
        if (currentStatus == BossState.Defeated || currentHP == maxHP) return;

        currentHP = maxHP;
        currentStatus = BossState.Normal;

        // 혹시 그로기 코루틴이 돌고 있다면 중단
        StopAllCoroutines();

        // UI 업데이트를 위해 이벤트를 호출하거나 로그를 남김
        Debug.Log("<color=blue>[Boss] 플레이어 이탈로 인해 체력이 초기화되었습니다.</color>");
    }
}