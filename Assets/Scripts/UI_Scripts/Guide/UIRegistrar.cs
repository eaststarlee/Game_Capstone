using UnityEngine;
using System.Collections.Generic;

public class UIRegistrar : MonoBehaviour
{
    [System.Serializable]
    public class UIPair
    {
        public string uiName;         // StringActivator에서 부를 이름
        public GameObject panelObject; // 버튼/패널 오브젝트
    }

    [Header("등록할 UI 패널 리스트")]
    public List<UIPair> uiPairs = new List<UIPair>();

    private void Start()
    {
        RegisterAll();
    }

    private void RegisterAll()
    {
        if (StringActivator.Instance == null) return;

        foreach (var pair in uiPairs)
        {
            if (pair.panelObject != null && !string.IsNullOrEmpty(pair.uiName))
            {
                StringActivator.Instance.RegisterPanel(pair.uiName.Trim(), pair.panelObject);
            }
        }
    }
}