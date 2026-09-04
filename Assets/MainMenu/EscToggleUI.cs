using System.Collections.Generic;
using UnityEngine;

public class EscToggleUI : MonoBehaviour
{
    [Header("현재 활성화되어 있는 오브젝트")]
    public GameObject currentObject;

    [Header("ESC를 누르면 활성화할 오브젝트")]
    public GameObject nextObject;

    void Update()
    {
        // ESC 키를 눌렀을 때
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // currentObject가 존재하고 현재 활성화 상태일 경우
            if (currentObject != null && currentObject.activeSelf)
            {
                // 현재 오브젝트 비활성화
                currentObject.SetActive(false);

                if (nextObject != null)
                {
                    nextObject.SetActive(true);

                    // ⚡ 수정: isNeedSync 조건과 false 처리를 지우고, ESC를 열 때마다 안전하게 호출
                    if (StringActivator.Instance != null)
                    {
                        StringActivator.Instance.ForceSyncAllUnlockedUI();
                    }
                }
            }
        }
    }
}