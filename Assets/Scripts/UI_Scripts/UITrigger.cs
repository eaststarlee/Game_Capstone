using UnityEngine;

public class UITrigger : MonoBehaviour
{
    // 트리거될 때 활성화/비활성화할 오브젝트
    public GameObject targetObject;

    // 트리거 진입 시 오브젝트 활성화 여부
    public bool activateOnEnter = true;

    // 트리거에서 나갈 때 오브젝트 비활성화 여부
    public bool deactivateOnExit = false;

    // 트리거 진입
    private void OnTriggerEnter(Collider other)
    {
        // 필요한 경우 특정 태그만 반응하도록 필터링 가능
        // if(other.CompareTag("Player"))
        // {
        //     targetObject.SetActive(activateOnEnter);
        // }
        if (other.CompareTag("Player"))
        {
            targetObject.SetActive(activateOnEnter);
        }
    }

    // 트리거에서 나감
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            targetObject.SetActive(!deactivateOnExit ? targetObject.activeSelf : false);
        }
    }
}
