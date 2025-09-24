using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class KeyObjectPair
{
    public KeyCode key;      // 누를 키
    public GameObject obj;   // 활성화/비활성화할 오브젝트
}

public class ObjActivatingKeys : MonoBehaviour
{
    [Header("키와 오브젝트 매핑")]
    public List<KeyObjectPair> keyObjectPairs = new List<KeyObjectPair>();

    private void Update()
    {
        foreach (var pair in keyObjectPairs)
        {
            if (pair.obj == null) continue; // 오브젝트 없으면 스킵

            if (Input.GetKeyDown(pair.key))
            {
                // 누르면 오브젝트 활성화 상태 토글
                pair.obj.SetActive(!pair.obj.activeSelf);
                Debug.Log($"{pair.obj.name} 활성화 상태: {pair.obj.activeSelf}");
            }
        }
    }
}
