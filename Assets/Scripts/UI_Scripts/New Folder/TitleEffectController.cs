using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class TitleEffectController : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI titleText;
    public CanvasGroup canvasGroup;

    [Header("Animation Settings")]
    public float zoomSpeed = 0.1f;      // 커지는 속도
    public float fadeDuration = 2.0f;   // 페이드 인/아웃에 걸리는 시간
    public float stayDuration = 1.0f;   // 완전히 보일 때 멈춰있는 시간
    public string nextSceneName = "PrologueScene"; // 이동할 씬 이름

    void Start()
    {
        // 초기화: 투명도 0, 크기 기본
        canvasGroup.alpha = 0;
        titleText.transform.localScale = Vector3.one;

        StartCoroutine(PlayTitleSequence());
    }

    IEnumerator PlayTitleSequence()
    {
        // 1. 페이드 인 + 서서히 커지기 시작
        float elapsed = 0f;
        Vector3 initialScale = titleText.transform.localScale;
        Vector3 targetScale = initialScale * 1.2f; // 1.2배까지 커짐

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;

            // 투명도 조절
            canvasGroup.alpha = Mathf.Lerp(0, 1, t);
            // 크기 조절 (서서히 커짐)
            titleText.transform.localScale = Vector3.Lerp(initialScale, targetScale, t);

            yield return null;
        }

        // 2. 잠시 대기 (연출 유지)
        yield return new WaitForSeconds(stayDuration);

        // 3. 페이드 아웃 (크기는 계속 조금씩 커지게 유지하면 더 자연스러움)
        elapsed = 0f;
        float startAlpha = canvasGroup.alpha;
        Vector3 currentScale = titleText.transform.localScale;
        Vector3 finalScale = currentScale * 1.1f; // 아웃될 때도 살짝 더 커짐

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;

            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0, t);
            titleText.transform.localScale = Vector3.Lerp(currentScale, finalScale, t);

            yield return null;
        }

        // 4. 다음 씬(프롤로그)으로 이동
        SceneManager.LoadScene(nextSceneName);
    }
}