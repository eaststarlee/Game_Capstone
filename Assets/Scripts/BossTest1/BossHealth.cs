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
    public BossUIController uiController;

    [Header("타격 이펙트 설정")]
    public GameObject hitEffectPrefab;
    public float effectDestroyTime = 1.0f;

    [Header("상태 이벤트")]
    public Action OnGroggyStart;
    public Action OnGroggyEnd;

    [Header("클리어 시 비활성화할 오브젝트들")]
    public List<GameObject> objectsToDisableOnDefeat = new List<GameObject>();

    private CharacterController playerController;

    private void Awake()
    {
        currentHP = maxHP;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerController = player.GetComponent<CharacterController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        HandleCollision(other, false);
    }

    public void HandleCollision(Collider other, bool isWeakPoint)
    {
        InkProjectileController projectile = other.GetComponent<InkProjectileController>();
        if (projectile != null && projectile.inkType == InkType.Black)
        {
            Vector3 hitPoint = other.transform.position;
            if (hitEffectPrefab != null)
            {
                GameObject effect = Instantiate(hitEffectPrefab, hitPoint, Quaternion.identity);
                Destroy(effect, effectDestroyTime);
            }
            ProcessHit(1, isWeakPoint);
            Destroy(other.gameObject);
        }
    }

    public void ProcessHit(int damage, bool isWeakPoint)
    {
        if (currentStatus == BossState.Defeated) return;

        int finalDamage = damage;
        if (currentStatus == BossState.Groggy) finalDamage = damage * 10;
        else if (isWeakPoint)
        {
            StopAllCoroutines();
            StartCoroutine(EnterGroggyState());
        }

        currentHP -= finalDamage;
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

        if (uiController != null) uiController.SetVisible(false);
        if (playerController != null) playerController.enabled = true;

        // --- 보스 소환수 즉시 정리 ---
        BossController controller = GetComponent<BossController>();
        if (controller != null && controller.ghostSpawner != null)
        {
            controller.ghostSpawner.ClearAllDrones();
        }

        foreach (GameObject obj in objectsToDisableOnDefeat)
        {
            if (obj != null) obj.SetActive(false);
        }

        gameObject.SetActive(false);
    }

    public void ResetHP()
    {
        if (currentStatus == BossState.Defeated || currentHP == maxHP) return;
        currentHP = maxHP;
        currentStatus = BossState.Normal;
        StopAllCoroutines();
    }
}