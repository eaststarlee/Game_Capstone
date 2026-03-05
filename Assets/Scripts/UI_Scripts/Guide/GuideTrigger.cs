using UnityEngine;

public class GuideTrigger : MonoBehaviour
{
    [SerializeField] private string guideName;      // GuideUI용
    [SerializeField] private string extraUIName;   // StringActivator용 (영구 UI)

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 1. 기존 가이드 시스템 호출
            if (GuideUI.Instance != null)
                GuideUI.Instance.OpenGuide(guideName);

            // 2. 새로운 전역 활성화 시스템 호출
            if (StringActivator.Instance != null)
                StringActivator.Instance.Activate(extraUIName);

            Destroy(gameObject);
        }
    }
}