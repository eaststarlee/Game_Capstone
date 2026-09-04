using UnityEngine;
using UnityEngine.UI;

public class ScreenSizeBox : MonoBehaviour
{
    [Header("UI 토글 버튼")]
    public Button toggleUIButton;
    public GameObject targetUI;

    [Header("해상도 변경 버튼들")]
    public Button res720pButton;
    public Button res900pButton;
    public Button res1080pButton;
    public Button res1440pButton;
    public Button res2160pButton;

    [Header("전체화면 토글 버튼")]
    public Button fullscreenButton;
    public Image fullscreenStateImage;  // 전체화면일 때 보여줄 이미지

    private bool isOn = false;   // UI 상태

    void Start()
    {
        // UI 토글 버튼
        if (toggleUIButton != null)
            toggleUIButton.onClick.AddListener(ToggleUI);

        // 해상도 버튼들
        if (res720pButton != null) res720pButton.onClick.AddListener(() => ChangeResolution(1280, 720));
        if (res900pButton != null) res900pButton.onClick.AddListener(() => ChangeResolution(1600, 900));
        if (res1080pButton != null) res1080pButton.onClick.AddListener(() => ChangeResolution(1920, 1080));
        if (res1440pButton != null) res1440pButton.onClick.AddListener(() => ChangeResolution(2560, 1440));
        if (res2160pButton != null) res2160pButton.onClick.AddListener(() => ChangeResolution(3840, 2160));

        // 전체화면 버튼
        if (fullscreenButton != null)
            fullscreenButton.onClick.AddListener(ToggleFullscreen);

        // 저장된 설정 적용
        GameSettings.Instance.ApplyGlobalSettings();

        // 초기 UI 상태 동기화
        SyncWithTargetUI();
        UpdateFullscreenButtonUI(GameSettings.Instance.isFullscreen);
    }

    void Update()
    {
        // UI 상태 동기화
        SyncWithTargetUI();
    }

    // ===== UI 토글 관련 =====
    void ToggleUI()
    {
        isOn = !isOn;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (targetUI != null)
            targetUI.SetActive(isOn);

        UpdateUIButtonText();
    }

    void SyncWithTargetUI()
    {
        if (targetUI != null)
            isOn = targetUI.activeSelf;

        UpdateUIButtonText();
    }

    void UpdateUIButtonText()
    {
        if (toggleUIButton != null)
        {
            Text btnText = toggleUIButton.GetComponentInChildren<Text>();
            if (btnText != null)
                btnText.text = isOn ? "ON" : "OFF";
        }
    }

    // ===== 해상도 변경 =====
    void ChangeResolution(int width, int height)
    {
        // 해상도 인덱스 저장
        if (width == 1280 && height == 720) GameSettings.Instance.resolutionIndex = 0;
        else if (width == 1600 && height == 900) GameSettings.Instance.resolutionIndex = 1;
        else if (width == 1920 && height == 1080) GameSettings.Instance.resolutionIndex = 2;
        else if (width == 2560 && height == 1440) GameSettings.Instance.resolutionIndex = 3;
        else if (width == 3840 && height == 2160) GameSettings.Instance.resolutionIndex = 4;

        // 저장 및 적용
        GameSettings.Instance.SaveAndApply();

        Debug.Log($"해상도 변경: {width} x {height}");
    }

    // ===== 전체화면 토글 =====
    void ToggleFullscreen()
    {
        // 상태 저장
        GameSettings.Instance.isFullscreen =
            !GameSettings.Instance.isFullscreen;

        // 저장 및 적용
        GameSettings.Instance.SaveAndApply();

        Debug.Log("전체화면 상태: " +
            (GameSettings.Instance.isFullscreen ? "ON" : "OFF"));

        // UI 갱신
        UpdateFullscreenButtonUI(GameSettings.Instance.isFullscreen);
    }
    void UpdateFullscreenButtonUI(bool fullscreenState)
    {
        // 버튼 텍스트 갱신
        if (fullscreenButton != null)
        {
            Text btnText = fullscreenButton.GetComponentInChildren<Text>();
            if (btnText != null)
                btnText.text = fullscreenState ? "Fullscreen ON" : "Fullscreen OFF";
        }

        // 상태 이미지 갱신
        if (fullscreenStateImage != null)
            fullscreenStateImage.gameObject.SetActive(fullscreenState);
    }
}
