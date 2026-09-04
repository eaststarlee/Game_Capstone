using UnityEngine;

public class ShowUIOnTrigger : MonoBehaviour
{
    public enum TriggerAction
    {
        Activate,   // 활성화
        Deactivate  // 비활성화
    }

    [Header("설정")]
    public GameObject uiObject;
    public TriggerAction action = TriggerAction.Activate;

    private void OnTriggerEnter(Collider other)
    {
        if (uiObject == null) return;

        switch (action)
        {
            case TriggerAction.Activate:
                uiObject.SetActive(true);
                break;

            case TriggerAction.Deactivate:
                uiObject.SetActive(false);
                break;
        }
    }
} 