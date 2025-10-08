using UnityEngine;

[System.Serializable]
public class UIElement
{
    public GameObject panel;    // 실제 UI Panel
    public bool blocksInput = true; // true면 열리면 입력 차단
}

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [Header("UI 목록")]
    public UIElement[] allUI;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // 현재 입력을 차단해야 하는 UI가 열려 있는지 확인
    public bool isInputBlocked
    {
        get
        {
            foreach (var ui in allUI)
            {
                if (ui.panel != null && ui.panel.activeSelf && ui.blocksInput)
                    return true;
            }
            return false;
        }
    }

    // 개별 UI 열기
    public void OpenUI(GameObject panel)
    {
        foreach (var ui in allUI)
        {
            if (ui.panel == panel)
                ui.panel.SetActive(true);
        }
    }

    // 개별 UI 닫기
    public void CloseUI(GameObject panel)
    {
        foreach (var ui in allUI)
        {
            if (ui.panel == panel)
                ui.panel.SetActive(false);
        }
    }
}
