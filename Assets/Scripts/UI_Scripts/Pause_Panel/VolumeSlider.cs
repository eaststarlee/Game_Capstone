using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;

public class VolumeSlider : MonoBehaviour
{
    public AudioMixer masterMixer;    // Inspector에서 MasterMixer 연결
    public Slider volumeSlider;       // Inspector에서 Slider 연결
    public TMP_Text volumeText;       // TMP Text 연결 (단계 표시용)

    // 10단계 dB 값 테이블 (0~10 단계)
    private readonly float[] volumeDB = { -80f, -50f, -40f, -33f, -27f, -22f, -18f, -14f, -10f, -5f, 0f };

    void Start()
    {
        // 저장된 값 불러오기 (0~10 단계)
        int savedStep = PlayerPrefs.GetInt("MasterVolumeStep", 9); // 기본 최대 단계
        volumeSlider.value = savedStep;
        UpdateVolume(savedStep);

        // 슬라이더 이벤트 연결
        volumeSlider.onValueChanged.AddListener(UpdateVolume);
    }

    public void UpdateVolume(float sliderValue)
    {
        // 0~10 단계로 반올림
        int stepIndex = Mathf.RoundToInt(sliderValue);

        // AudioMixer에 단계별 dB 값 적용
        masterMixer.SetFloat("MasterVolume", volumeDB[stepIndex]);

        // Slider와 TMP 텍스트 업데이트
        volumeSlider.value = stepIndex;
        volumeText.text = stepIndex.ToString(); // 0~10 표시

        // 저장
        PlayerPrefs.SetInt("MasterVolumeStep", stepIndex);
    }
}
