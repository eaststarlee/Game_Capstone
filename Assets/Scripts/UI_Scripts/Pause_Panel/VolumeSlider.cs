using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VolumeSlider : MonoBehaviour
{
    public Slider volumeSlider;       // Inspector에서 Slider 연결
    public TMP_Text volumeText;       // TMP Text 연결 (단계 표시용)

    void Start()
    {
        // 저장된 단계 불러오기 (기본값 9단계)
        int savedStep = PlayerPrefs.GetInt("GameSettings.Instance.masterVolumeStep", 9);

        // 슬라이더 범위 설정 (코드에서 강제하거나 인스펙터에서 0~10 설정)
        volumeSlider.minValue = 0;
        volumeSlider.maxValue = 10;
        volumeSlider.wholeNumbers = true; // 정수 단위로만 움직이게 설정

        volumeSlider.value = savedStep;
        UpdateVolume(savedStep);

        // 슬라이더 이벤트 연결
        volumeSlider.onValueChanged.AddListener(UpdateVolume);
    }

    public void UpdateVolume(float sliderValue)
    {
        int stepIndex = Mathf.RoundToInt(sliderValue);

        // 핵심: AudioListener의 볼륨을 0.0 ~ 1.0 사이로 설정
        // 10단계 기준이므로 stepIndex / 10f를 해줍니다.
        AudioListener.volume = stepIndex / 10f;

        // UI 업데이트
        volumeText.text = stepIndex.ToString();

        // 설정 저장
        PlayerPrefs.SetInt("GameSettings.Instance.masterVolumeStep", stepIndex);
    }
}