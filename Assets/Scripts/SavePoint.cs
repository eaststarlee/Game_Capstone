using UnityEngine;
using System;

public class SavePoint : MonoBehaviour
{
    private Collider triggerCollider;

    public static event Action OnSaveTriggered; 

    [Header("Sound Effects")]
    public AudioClip saveSfx;
    [Range(0f, 1f)] public float saveSfxVolume = 1f;

    // 플레이어 감지용
    private bool isPlayerInRange = false;
    private PlayerHealth currentPlayerHealth;

    [Header("Respawn Adjustment")]
    [Tooltip("세이브 포인트 중심 좌표 기준, 위로 얼만큼 띄워서 리스폰 시킬지 오프셋 (단위: 유닛)")]
    public float respawnHeightOffset = 1.0f;

    private void Start()
    {
        triggerCollider = GetComponent<Collider>();
        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
            Debug.Log("[SavePoint] Collider가 Trigger로 설정됨");
        }
        else
        {
            Debug.LogError("[SavePoint] Collider 컴포넌트가 없습니다!");
        }
    }

    private void Update()
    {
        // 범위 안에 있고, F키가 최초로 눌린 딱 1프레임에 처리
        if (isPlayerInRange && currentPlayerHealth != null && Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("[SavePoint] F 키 감지됨! (단발)");

            // 🚨 방법 B 적용: 콜라이더가 그려내는 실제 바운딩 박스 정중앙의 발밑 부분 추출
            Vector3 colliderCenter = triggerCollider.bounds.center;
            float colliderBottomY = triggerCollider.bounds.min.y;
            
            // X, Z는 콜라이더 정중앙, Y는 콜라이더 바닥면 + (사용자 지정 오프셋)
            Vector3 adjustedRespawnPoint = new Vector3(colliderCenter.x, colliderBottomY, colliderCenter.z) 
                                         + Vector3.up * respawnHeightOffset;

            currentPlayerHealth.SetRespawnPoint(adjustedRespawnPoint);
            Debug.Log("[SavePoint] 세이브 완료: " + adjustedRespawnPoint);
            OnSaveTriggered?.Invoke();

            if (saveSfx != null)
            {
                GameObject sfxObj = new GameObject("SaveSfx");
                sfxObj.transform.position = adjustedRespawnPoint; // 소리 나는 위치도 보정
                AudioSource source = sfxObj.AddComponent<AudioSource>();
                source.clip = saveSfx;
                source.spatialBlend = 0f; // 2D (거리 무관)
                source.volume = saveSfxVolume;
                source.Play();
                Destroy(sfxObj, saveSfx.length + 0.1f);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            currentPlayerHealth = other.GetComponent<PlayerHealth>();
            Debug.Log("[SavePoint] 플레이어가 범위에 들어왔습니다");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            currentPlayerHealth = null;
            Debug.Log("[SavePoint] 플레이어가 범위에서 나갔습니다");
        }
    }

    // Draw a gizmo in the editor to visualize the save point
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }
}


