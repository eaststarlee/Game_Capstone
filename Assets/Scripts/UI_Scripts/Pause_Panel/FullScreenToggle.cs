using UnityEngine;
using UnityEngine.UI;

public class FullscreenToggle : MonoBehaviour
{
    public Button toggleButton;    // 인스펙터에서 버튼 연결

    void Start()
    {
        // 버튼 클릭 이벤트 연결
        if (toggleButton != null)
            toggleButton.onClick.AddListener(ToggleFullscreen);

        UpdateButtonText();
    }

    void Update()
    {
        // 매 프레임마다 전체화면 상태를 동기화
        UpdateButtonText();
    }

    void ToggleFullscreen()
    {
        // 현재 상태 반전
        bool isFullscreen = !Screen.fullScreen;

        // 전체화면 적용
        Screen.fullScreen = isFullscreen;

        UpdateButtonText();
        Debug.Log("전체화면 상태: " + (isFullscreen ? "ON" : "OFF"));
    }

    void UpdateButtonText()
    {
        if (toggleButton != null)
        {
            Text btnText = toggleButton.GetComponentInChildren<Text>();
            if (btnText != null)
                btnText.text = Screen.fullScreen ? "Fullscreen ON" : "Fullscreen OFF";
        }
    }
}
