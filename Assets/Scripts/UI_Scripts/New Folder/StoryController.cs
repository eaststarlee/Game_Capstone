using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class StoryController : MonoBehaviour
{
    [Header("UI Elements")]
    public Image[] fadeImages;
    public TextMeshProUGUI storyText;
    public Image blackCurtain;

    [Header("Story Data")]
    public Sprite[] storySprites;
    [TextArea] public string[] storyScripts;

    [Header("Settings")]
    public float crossFadeDuration = 2.0f;
    public float[] slideDurations;
    public string nextSceneName = "Stage1";

    [Header("Curtain Fade Settings")]
    public float startCurtainFadeDuration = 1.0f;
    public float endCurtainFadeDuration = 3.0f;

    private int currentImageIndex = 0;

    void Start()
    {
        blackCurtain.canvasRenderer.SetAlpha(1.0f);
        fadeImages[0].canvasRenderer.SetAlpha(0.0f);
        fadeImages[1].canvasRenderer.SetAlpha(0.0f);

        StartCoroutine(PlayStory());
    }

    IEnumerator PlayStory()
    {
        currentImageIndex = 0;
        fadeImages[0].sprite = storySprites[0];
        storyText.text = storyScripts[0];
        storyText.alpha = 1.0f;
        fadeImages[0].canvasRenderer.SetAlpha(1.0f);

        yield return new WaitForSeconds(0.5f);

        yield return StartCoroutine(FadeCurtain(0f, startCurtainFadeDuration));

        float firstDuration = (slideDurations.Length > 0) ? slideDurations[0] : 2.0f;
        yield return new WaitForSeconds(firstDuration);

        for (int i = 1; i < storySprites.Length; i++)
        {
            Image outUI = fadeImages[currentImageIndex];
            currentImageIndex = (currentImageIndex + 1) % 2;
            Image inUI = fadeImages[currentImageIndex];

            inUI.sprite = storySprites[i];

            // 텍스트 페이드와 이미지 페이드를 동시에 실행
            StartCoroutine(CrossFadeText(storyScripts[i]));
            yield return StartCoroutine(CrossFade(outUI, inUI));

            float currentDuration = (slideDurations.Length > i) ? slideDurations[i] : 2.0f;
            yield return new WaitForSeconds(currentDuration);
        }

        yield return StartCoroutine(LoadNextSceneAsync());
    }

    // 자막을 스르륵 교체하는 코루틴
    IEnumerator CrossFadeText(string nextText)
    {
        float halfDuration = crossFadeDuration / 2f;
        float elapsed = 0f;

        // 기존 자막 페이드 아웃
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            storyText.alpha = 1.0f - (elapsed / halfDuration);
            yield return null;
        }

        // 내용 교체
        storyText.text = nextText;

        // 새 자막 페이드 인
        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            storyText.alpha = elapsed / halfDuration;
            yield return null;
        }

        storyText.alpha = 1.0f;
    }

    IEnumerator FadeCurtain(float targetAlpha, float duration)
    {
        float elapsed = 0f;
        float startAlpha = blackCurtain.canvasRenderer.GetAlpha();

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            blackCurtain.canvasRenderer.SetAlpha(Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration));
            yield return null;
        }
        blackCurtain.canvasRenderer.SetAlpha(targetAlpha);
    }

    IEnumerator CrossFade(Image outImage, Image inImage)
    {
        float elapsed = 0f;
        while (elapsed < crossFadeDuration)
        {
            elapsed += Time.deltaTime;
            float percent = elapsed / crossFadeDuration;
            outImage.canvasRenderer.SetAlpha(1.0f - percent);
            inImage.canvasRenderer.SetAlpha(percent);
            yield return null;
        }
        outImage.canvasRenderer.SetAlpha(0f);
        inImage.canvasRenderer.SetAlpha(1f);
    }

    IEnumerator LoadNextSceneAsync()
    {
        yield return StartCoroutine(FadeCurtain(1f, endCurtainFadeDuration));

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(nextSceneName);
        asyncLoad.allowSceneActivation = true;
    }
}