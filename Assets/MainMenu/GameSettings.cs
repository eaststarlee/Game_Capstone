using UnityEngine;

public class GameSettings : MonoBehaviour
{
    public static GameSettings Instance;

    [Header("감도 설정 (1~10 단계)")]
    [Range(1, 10)]
    public int sensitivityStep = 5;

    [Header("음량 설정 (0~10 단계)")]
    [Range(0, 10)]
    public int masterVolumeStep = 9;

    [Header("밝기 설정 (1~10 단계)")]
    [Range(1, 10)]
    public int brightnessStep = 10;

    [Header("화면 설정")]
    public bool isFullscreen = true;

    // 0 = 1280x720
    // 1 = 1600x900
    // 2 = 1920x1080
    // 3 = 2560x1440
    // 4 = 3840x2160
    [Range(0, 4)]
    public int resolutionIndex = 2;

    private void Awake()
    {
        // 싱글톤 처리
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadSettings();
        ApplyGlobalSettings();
    }

    // =========================
    // 저장
    // =========================
    public void SaveSettings()
    {
        PlayerPrefs.SetInt("SensitivityStep", sensitivityStep);
        PlayerPrefs.SetInt("MasterVolumeStep", masterVolumeStep);
        PlayerPrefs.SetInt("BrightnessStep", brightnessStep);
        PlayerPrefs.SetInt("IsFullscreen", isFullscreen ? 1 : 0);
        PlayerPrefs.SetInt("ResolutionIndex", resolutionIndex);

        PlayerPrefs.Save();
    }

    // =========================
    // 불러오기
    // =========================
    public void LoadSettings()
    {
        sensitivityStep = PlayerPrefs.GetInt("SensitivityStep", 5);
        masterVolumeStep = PlayerPrefs.GetInt("MasterVolumeStep", 9);
        brightnessStep = PlayerPrefs.GetInt("BrightnessStep", 10);
        isFullscreen = PlayerPrefs.GetInt("IsFullscreen", 1) == 1;
        resolutionIndex = PlayerPrefs.GetInt("ResolutionIndex", 2);
    }

    // =========================
    // 감도 계산
    // =========================
    public float GetCalculatedSensitivity()
    {
        switch (sensitivityStep)
        {
            case 1: return 5f;
            case 2: return 10f;
            case 3: return 25f;
            case 4: return 50f;
            case 5: return 100f;
            case 6: return 150f;
            case 7: return 250f;
            case 8: return 350f;
            case 9: return 450f;
            case 10: return 550f;
            default: return 100f;
        }
    }

    // =========================
    // 해상도 가져오기
    // =========================
    public Vector2Int GetResolution()
    {
        switch (resolutionIndex)
        {
            case 0: return new Vector2Int(1280, 720);
            case 1: return new Vector2Int(1600, 900);
            case 2: return new Vector2Int(1920, 1080);
            case 3: return new Vector2Int(2560, 1440);
            case 4: return new Vector2Int(3840, 2160);
            default: return new Vector2Int(1920, 1080);
        }
    }

    // =========================
    // 전역 설정 즉시 적용
    // =========================
    public void ApplyGlobalSettings()
    {
        // 음량 적용
        AudioListener.volume = masterVolumeStep / 10f;

        // 해상도 및 전체화면 적용
        Vector2Int res = GetResolution();
        Screen.SetResolution(res.x, res.y, isFullscreen);
    }

    // =========================
    // 설정 변경 후 호출용
    // =========================
    public void SaveAndApply()
    {
        SaveSettings();
        ApplyGlobalSettings();
    }
}