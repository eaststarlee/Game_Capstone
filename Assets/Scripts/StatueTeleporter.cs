using UnityEngine;

public class StatueTeleporter : MonoBehaviour
{
    public Vector3 teleportDestination = new Vector3(-141f, -140f, -167f); // User-provided coordinates
    public bool enableSavePoint = true; // Option to also act as a save point

    private Collider triggerCollider;

    private void Start()
    {
        triggerCollider = GetComponent<Collider>();
        if (triggerCollider != null)
        {
            // Ensure the collider is a trigger to detect player entry
            triggerCollider.isTrigger = true;
            Debug.Log("[StatueTeleporter] Collider가 Trigger로 설정됨");
        }
        else
        {
            Debug.LogError("[StatueTeleporter] Collider 컴포넌트가 없습니다! 이 스크립트가 작동하려면 Trigger Collider가 필요합니다.");
            enabled = false; // Disable script if no collider is found
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // Check if the colliding object is the player
        if (other.CompareTag("Player"))
        {
            // Check if the 'F' key is pressed
            if (Input.GetKeyDown(KeyCode.F))
            {
                Debug.Log("[StatueTeleporter] F 키 감지됨!");

                // Handle save point functionality if enabled
                if (enableSavePoint)
                {
                    PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
                    if (playerHealth != null)
                    {
                        playerHealth.SetRespawnPoint(transform.position);
                        Debug.Log("[StatueTeleporter] 세이브 완료: " + transform.position);
                    }
                    else
                    {
                        Debug.LogWarning("[StatueTeleporter] PlayerHealth 컴포넌트를 찾을 수 없습니다! 세이브 포인트 기능을 사용할 수 없습니다.");
                    }
                }

                // Handle teleportation
                CharacterController cc = other.GetComponent<CharacterController>();
                if (cc != null)
                {
                    cc.enabled = false; // Disable controller before teleporting
                    other.transform.position = teleportDestination; // Teleport player
                    cc.enabled = true; // Re-enable controller
                    Debug.Log($"[StatueTeleporter] 플레이어를 {teleportDestination}로 순간이동했습니다.");
                }
                else
                {
                    other.transform.position = teleportDestination; // Teleport player without CharacterController
                    Debug.Log($"[StatueTeleporter] CharacterController 없이 플레이어를 {teleportDestination}로 순간이동했습니다.");
                }
            }
        }
    }

    // Draw gizmos for easier visualization in the editor
    private void OnDrawGizmos()
    {
        // Visualize the statue's trigger area (assuming it has a collider)
        Gizmos.color = Color.yellow;
        Collider ownCollider = GetComponent<Collider>();
        if (ownCollider != null)
        {
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale);
            Gizmos.DrawWireCube(ownCollider.bounds.center - transform.position, ownCollider.bounds.size);
            Gizmos.matrix = Matrix4x4.identity; // Reset Gizmo matrix
        }
        else
        {
            // If no collider, just draw a sphere at object's position
            Gizmos.DrawWireSphere(transform.position, 1f); 
        }

        // Visualize the teleport destination
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(teleportDestination, 0.5f); // Draw a sphere at destination
        Gizmos.DrawLine(transform.position, teleportDestination); // Draw a line from statue to destination
    }
}
