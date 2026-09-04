using System.Collections.Generic;
using UnityEngine;

public class TalkBox : MonoBehaviour
{
    public List<GameObject> aObjects = new List<GameObject>(); // 활성화할 오브젝트
    public List<GameObject> bObjects = new List<GameObject>(); // 비활성화/활성화될 오브젝트

    private void Update()
    {
        // 리스트 길이 일치 여부 체크
        if (aObjects.Count != bObjects.Count)
        {
            Debug.LogWarning("aObjects와 bObjects의 길이가 일치하지 않습니다.");
            return;
        }

        for (int i = 0; i < aObjects.Count; i++)
        {
            GameObject a = aObjects[i];
            GameObject b = bObjects[i];

            if (a == null || b == null) continue;

            if (a.activeSelf)
            {
                // F 키 외 다른 키 입력 차단
                if (Input.anyKeyDown && !Input.GetKeyDown(KeyCode.F))
                {
                    Debug.Log("F키 외 입력 잠금");
                }

                // a가 활성화되어 있으면 b는 비활성화
                if (b.activeSelf)
                {
                    b.SetActive(false);
                    Debug.Log($"{b.name} 비활성화 완료");
                }
            }
            else
            {
                // a가 비활성화되어 있으면 b는 활성화
                if (!b.activeSelf)
                {
                    b.SetActive(true);
                    Debug.Log($"{b.name} 활성화 완료");
                }
            }
        }
    }
}
