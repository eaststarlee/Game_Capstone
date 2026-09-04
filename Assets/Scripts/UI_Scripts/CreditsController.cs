using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class CreditsController : MonoBehaviour
{
    [Header("UI 연결")]
    public TextMeshProUGUI creditText;

    [Header("크레딧 내용")]
    [TextArea(2, 5)]
    public string[] creditLines;

    [Header("시간 설정 (초)")]
    public float fadeInTime = 1.5f;
    public float[] displayDurations;
    public float fadeOutTime = 1.5f;
    public float waitBetweenLines = 0.5f;

    [Header("종료 후 이동할 씬")]
    public string nextSceneName = "MainMenu";

    // BGM 페이드아웃 설정
    [Header("오디오 설정")]
    public AudioSource bgmSource;
    public float bgmFadeDuration = 2.0f; 

    void Start()
    {
        SetTextAlpha(0f);
        StartCoroutine(PlayCredits());
    }

    IEnumerator PlayCredits()
    {
        yield return new WaitForSeconds(1.0f);

        for (int i = 0; i < creditLines.Length; i++)
        {
            creditText.text = creditLines[i];

            // 1. 나타나기
            yield return StartCoroutine(FadeText(1f, fadeInTime));

            // 2. 가만히 보여주기
            float currentDuration = (displayDurations.Length > i) ? displayDurations[i] : 2.0f;
            yield return new WaitForSeconds(currentDuration);

            // 3. 사라지기
            yield return StartCoroutine(FadeText(0f, fadeOutTime));

            // 4. 다음 글자 나오기 전 잠깐 쉬기
            yield return new WaitForSeconds(waitBetweenLines);
        }

        if (bgmSource != null)
        {
            yield return StartCoroutine(FadeOutBGM());
        }
        else
        {
            yield return new WaitForSeconds(2.0f);
        }

        SceneManager.LoadScene(nextSceneName);
    }

    IEnumerator FadeText(float targetAlpha, float duration)
    {
        float startAlpha = creditText.color.a;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
            SetTextAlpha(newAlpha);
            yield return null;
        }
        SetTextAlpha(targetAlpha);
    }

    void SetTextAlpha(float alpha)
    {
        Color c = creditText.color;
        c.a = alpha;
        creditText.color = c;
    }

    IEnumerator FadeOutBGM()
    {
        float startVolume = bgmSource.volume;
        float elapsed = 0f;

        while (elapsed < bgmFadeDuration)
        {
            elapsed += Time.deltaTime;
            // 볼륨을 원래 크기에서 0까지 시간에 맞춰 줄임
            bgmSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / bgmFadeDuration);
            yield return null;
        }

        bgmSource.volume = 0f;
    }
}