using UnityEngine;
using System.Collections;

public class SavePointUI : MonoBehaviour
{
    [Header("Inspector에서 연결할 UI")]
    public GameObject saveUIPopup; // Inspector에서 UI 오브젝트 연결
    public float displayTime = 2f; // UI 표시 시간

    private void OnEnable()
    {
        SavePoint.OnSaveTriggered += ShowSaveUI; // 이벤트 구독
    }

    private void OnDisable()
    {
        SavePoint.OnSaveTriggered -= ShowSaveUI; // 이벤트 해제
    }

    private void ShowSaveUI()
    {
        if (saveUIPopup != null)
        {
            saveUIPopup.SetActive(true); // UI 켬
            StartCoroutine(HideUICoroutine());
        }
        else
        {
            Debug.LogWarning("[SavePointUI] UI가 연결되지 않았습니다!");
        }
    }

    private IEnumerator HideUICoroutine()
    {
        yield return new WaitForSeconds(displayTime);
        if (saveUIPopup != null)
            saveUIPopup.SetActive(false); // UI 끔
    }
}
