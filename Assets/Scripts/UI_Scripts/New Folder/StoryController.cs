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
    // [추가] 최상단에서 화면을 가려줄 검은 이미지
    public Image blackCurtain;

    [Header("Story Data")]
    public Sprite[] storySprites;
    [TextArea] public string[] storyScripts;

    [Header("Settings")]
    public float crossFadeDuration = 2.0f;
    public float displayTime = 2.0f;
    public string nextSceneName = "Stage1";
    // [추가] 커튼이 걷히는 속도
    public float curtainFadeDuration = 1.0f;

    private int currentImageIndex = 0;

    void Start()
    {
        // 1. 초기 상태: 커튼은 불투명(1), 스토리 이미지들은 투명(0)
        blackCurtain.canvasRenderer.SetAlpha(1.0f);
        fadeImages[0].canvasRenderer.SetAlpha(0.0f);
        fadeImages[1].canvasRenderer.SetAlpha(0.0f);

        StartCoroutine(PlayStory());
    }

    IEnumerator PlayStory()
    {
        // --- [추가] 첫 시작 연출: 커튼 뒤에서 첫 이미지를 미리 준비 ---
        currentImageIndex = 0;
        fadeImages[0].sprite = storySprites[0];
        storyText.text = storyScripts[0];
        fadeImages[0].canvasRenderer.SetAlpha(1.0f); // 커튼 뒤에서 이미 켜둠

        // 잠시 대기 (안정화)
        yield return new WaitForSeconds(0.5f);

        // 커튼을 페이드 아웃 시켜서 첫 장면 공개
        yield return StartCoroutine(FadeCurtain(0f));

        // 첫 장면을 보여준 후 유지 시간만큼 대기
        yield return new WaitForSeconds(displayTime);
        // --------------------------------------------------------

        // 두 번째 장(index 1)부터 크로스페이드 루프 시작
        for (int i = 1; i < storySprites.Length; i++)
        {
            Image outUI = fadeImages[currentImageIndex];
            currentImageIndex = (currentImageIndex + 1) % 2;
            Image inUI = fadeImages[currentImageIndex];

            inUI.sprite = storySprites[i];
            storyText.text = storyScripts[i];

            yield return StartCoroutine(CrossFade(outUI, inUI));
            yield return new WaitForSeconds(displayTime);
        }

        yield return StartCoroutine(LoadNextSceneAsync());
    }

    // [추가] 커튼 전용 페이드 함수
    IEnumerator FadeCurtain(float targetAlpha)
    {
        float elapsed = 0f;
        float startAlpha = blackCurtain.canvasRenderer.GetAlpha();

        while (elapsed < curtainFadeDuration)
        {
            elapsed += Time.deltaTime;
            blackCurtain.canvasRenderer.SetAlpha(Mathf.Lerp(startAlpha, targetAlpha, elapsed / curtainFadeDuration));
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
        // 씬 넘어가기 전 다시 커튼을 쳐서 어둡게 만듬
        yield return StartCoroutine(FadeCurtain(1f));

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(nextSceneName);
        asyncLoad.allowSceneActivation = true;
    }
}