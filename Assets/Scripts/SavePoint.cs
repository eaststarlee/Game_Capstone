using UnityEngine;
using System;

public class SavePoint : MonoBehaviour
{
    private Collider triggerCollider;

    public static event Action OnSaveTriggered; 

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

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("[SavePoint] 플레이어가 범위에 들어왔습니다");
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // Check if the colliding object is the player
        if (other.CompareTag("Player"))
        {
            // Check if the 'F' key is pressed
            if (Input.GetKey(KeyCode.F))
            {
                Debug.Log("[SavePoint] F 키 감지됨!");
                
                PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.SetRespawnPoint(transform.position);
                    Debug.Log("[SavePoint] 세이브 완료: " + transform.position);
                    OnSaveTriggered?.Invoke();
                }
                else
                {
                    Debug.LogWarning("[SavePoint] PlayerHealth 컴포넌트를 찾을 수 없습니다!");
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
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


