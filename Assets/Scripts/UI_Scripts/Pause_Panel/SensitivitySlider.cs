using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SensitivitySlider : MonoBehaviour
{
    [Header("UI 연결")]
    public Slider sensitivitySlider;   // 1~10단계 슬라이더
    public TMP_Text sensitivityText;   // 값만 표시

    [Header("게임 내 감도 기준")]
    public float baseSensitivity = 20f;       // PlayerController 기준 기본값
    public float sensitivityMultiplier = 5f;  // 전체 감도 강제 배율 (step1~5용)

    [HideInInspector]
    public float sliderValue;                // 현재 슬라이더 값

    private void Start()
    {
        // 슬라이더 설정
        sensitivitySlider.minValue = 1;
        sensitivitySlider.maxValue = 10;
        sensitivitySlider.wholeNumbers = true;

        // 기본값 5단계
        sensitivitySlider.value = 5;
        sliderValue = sensitivitySlider.value;

        sensitivitySlider.onValueChanged.AddListener(UpdateSliderValue);

        // 초기 텍스트 표시
        if (sensitivityText != null)
            sensitivityText.text = sliderValue.ToString("0");
    }

    private void UpdateSliderValue(float value)
    {
        sliderValue = value;

        // 텍스트 갱신
        if (sensitivityText != null)
            sensitivityText.text = sliderValue.ToString("0");
    }

    // --- PlayerController에서 읽어 적용할 감도 계산 ---
    public float GetCalculatedSensitivity()
    {
        int step = Mathf.RoundToInt(sliderValue);
        float sensitivity;

        switch (step)
        {
            // 1~5 단계 강제값
            case 1: sensitivity = 5f; break;
            case 2: sensitivity = 10f; break;
            case 3: sensitivity = 25f; break;
            case 4: sensitivity = 50f; break;
            case 5: sensitivity = 100f; break;

            // 6~10 단계 기존 값 유지
            case 6: sensitivity = 150f; break;
            case 7: sensitivity = 250f; break;
            case 8: sensitivity = 350f; break;
            case 9: sensitivity = 450f; break;
            case 10: sensitivity = 550f; break;

            default: sensitivity = 100f; break; // 안전용
        }

        return sensitivity;
    }

}
