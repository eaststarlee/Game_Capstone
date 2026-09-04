using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BrightnessSlider : MonoBehaviour
{
    [Header("UI Elements")]
    public Slider brightnessSlider;
    public TMP_Text brightnessText;
    public Image uiDarkOverlay; // UI를 어둡게 덮는 Overlay 이미지 (검은색)

    [Header("Post Processing")]
    public Volume postProcessingVolume;
    private ColorAdjustments colorAdjustments;

    [Header("Exposure Settings")]
    public float minExposure = -2f; // 가장 어두운 단계
    public float maxExposure = 0f;  // 기본 밝기

    [Header("Overlay Settings")]
    [Range(0f, 1f)]
    public float minOverlayAlpha = 0f;   // 밝을 때 알파
    [Range(0f, 1f)]
    public float maxOverlayAlpha = 0.5f; // 어두울 때 알파 (최대치 제한)

    private void Start()
    {
        // Profile에서 Color Adjustments 찾기
        if (postProcessingVolume.profile.TryGet<ColorAdjustments>(out var ca))
            colorAdjustments = ca;
        else
            Debug.LogError("Color Adjustments가 Volume Profile에 없습니다.");

        // 슬라이더 설정
        brightnessSlider.minValue = 1;
        brightnessSlider.maxValue = 10;
        brightnessSlider.wholeNumbers = true;

        // UI Overlay가 클릭 막지 않도록 설정
        if (uiDarkOverlay != null)
            uiDarkOverlay.raycastTarget = false;

        // 저장된 값 불러오기
        brightnessSlider.value = GameSettings.Instance.brightnessStep;

        // 밝기 적용
        UpdateBrightness(brightnessSlider.value);

        brightnessSlider.onValueChanged.AddListener(UpdateBrightness);
    }

    private void UpdateBrightness(float value)
    {
        GameSettings.Instance.brightnessStep = Mathf.RoundToInt(value);
        GameSettings.Instance.SaveSettings();

        if (colorAdjustments == null) return;

        float normalized = (value - 1) / 9f;

        colorAdjustments.postExposure.value =
            Mathf.Lerp(minExposure, maxExposure, normalized);

        if (uiDarkOverlay != null)
        {
            float alpha =
                Mathf.Lerp(maxOverlayAlpha, minOverlayAlpha, normalized);

            uiDarkOverlay.color = new Color(0, 0, 0, alpha);
        }

        if (brightnessText != null)
            brightnessText.text = value.ToString("0");
    }
}
