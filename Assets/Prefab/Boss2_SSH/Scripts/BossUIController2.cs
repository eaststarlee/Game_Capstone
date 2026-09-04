using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class BossUIController2 : MonoBehaviour
{
    [Header("보스 연결")]
    public BossHealth2 bossHealth;

    [Header("UI 요소")]
    public GameObject hpCanvasRoot;
    public Image mainFillImage;      // 메인 체력바
    public Image spectrumFillImage;  // 잔상 체력바
    public TextMeshProUGUI percentageText;
    public TextMeshProUGUI patternWarningText; // 패턴 안내 메시지

    [Header("설정")]
    public float lerpSpeed = 5f;

    private void Update()
    {
        if (bossHealth == null || !hpCanvasRoot.activeSelf) return;

        float targetFill = (float)bossHealth.currentHP / bossHealth.maxHP;

        // 보스 1의 부드러운 게이지 연출 계승
        mainFillImage.fillAmount = Mathf.Lerp(mainFillImage.fillAmount, targetFill, Time.deltaTime * lerpSpeed * 2f);
        spectrumFillImage.fillAmount = Mathf.Lerp(spectrumFillImage.fillAmount, targetFill, Time.deltaTime * lerpSpeed);

        if (percentageText != null)
            percentageText.text = $"{(targetFill * 100f):F0}%";
    }

    public void SetVisible(bool visible)
    {
        if (hpCanvasRoot != null) hpCanvasRoot.SetActive(visible);
    }

    public void ShowPatternMessage(string message)
    {
        if (patternWarningText != null)
        {
            StopAllCoroutines();
            StartCoroutine(DisplayMessageRoutine(message));
        }
    }

    private IEnumerator DisplayMessageRoutine(string message)
    {
        patternWarningText.text = message;
        patternWarningText.gameObject.SetActive(true);
        yield return new WaitForSeconds(5f);
        patternWarningText.gameObject.SetActive(false);
    }
}