using UnityEngine;
using System.Collections.Generic;

public class UIRegistrar : MonoBehaviour
{
    // 이름과 패널을 짝짓기 위한 데이터 구조
    [System.Serializable]
    public class UIPair
    {
        public string uiName;        // StringActivator에서 부를 이름
        public GameObject panelObject; // 실제 비활성화된 패널 오브젝트
    }

    [Header("등록할 UI 패널 리스트")]
    public List<UIPair> uiPairs = new List<UIPair>();

    private void Awake()
    {
        // Manager(StringActivator)가 먼저 생성되기를 기다리는 것이 안전하므로 
        // 씬 로드 직후 바로 등록을 수행합니다.
        RegisterAll();
    }

    private void RegisterAll()
    {
        if (StringActivator.Instance == null)
        {
            Debug.LogWarning("[UIRegistrar] StringActivator 인스턴스를 찾을 수 없습니다!");
            return;
        }

        foreach (var pair in uiPairs)
        {
            if (pair.panelObject != null && !string.IsNullOrEmpty(pair.uiName))
            {
                // StringActivator에게 이름과 실제 오브젝트(꺼져있어도 상관없음)를 전달
                StringActivator.Instance.RegisterPanel(pair.uiName, pair.panelObject);
            }
        }

        Debug.Log($"[UIRegistrar] 총 {uiPairs.Count}개의 패널 대리 등록 완료.");
    }
}