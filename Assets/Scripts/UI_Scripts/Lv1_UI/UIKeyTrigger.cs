using UnityEngine;

public class UIKeyTrigger : MonoBehaviour
{
    public GameObject aObject; // F 누르면 활성화할 오브젝트
    public GameObject bObject; // F 누르면 비활성화할 오브젝트

    private bool playerInTrigger = false;

    private void Update()
    {
        if (playerInTrigger && Input.GetKeyDown(KeyCode.F))
        {
            if (aObject != null)
                aObject.SetActive(true);

            if (bObject != null)
                bObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTrigger = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTrigger = false;

            // 트리거 벗어나면 원래 상태 복원
            if (aObject != null)
                aObject.SetActive(false);

            if (bObject != null)
                bObject.SetActive(true);
        }
    }
}
