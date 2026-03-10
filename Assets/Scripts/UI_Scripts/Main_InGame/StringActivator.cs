using UnityEngine;
using System.Collections.Generic;

public class StringActivator : MonoBehaviour
{
    public static StringActivator Instance;

    // 등록된 UI 패널들을 이름으로 저장하는 창고
    private Dictionary<string, GameObject> uiPanels = new Dictionary<string, GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // UI_Root 등 최상위 오브젝트가 있다면 그것까지 파괴 방지
            DontDestroyOnLoad(transform.root.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // UI 패널이 깨어날 때 스스로를 등록하는 함수
    public void RegisterPanel(string uiName, GameObject panelObj)
    {
        if (!uiPanels.ContainsKey(uiName))
        {
            uiPanels.Add(uiName, panelObj);
            Debug.Log($"[StringActivator] '{uiName}' 등록 완료.");
        }
    }

    // 이름으로 UI를 찾아 활성화
    public void Activate(string targetName)
    {
        if (uiPanels.TryGetValue(targetName, out GameObject target))
        {
            target.SetActive(true);
            Debug.Log($"[StringActivator] '{targetName}'을(를) 활성화했습니다.");
        }
        else
        {
            Debug.LogWarning($"[StringActivator] '{targetName}'이(가) 등록되지 않았습니다. 이름을 확인하세요.");
        }
    }
}