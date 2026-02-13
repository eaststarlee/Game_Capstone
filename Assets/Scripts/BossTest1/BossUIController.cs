using UnityEngine;
using UnityEngine.UI; // Image 컴포넌트 제어용
using TMPro;

public class BossUIController : MonoBehaviour
{
    [Header("연결할 보스 스크립트")]
    public BossHealth bossHealth;

    [Header("UI 루트 오브젝트")]
    public GameObject hpCanvasRoot; // HP바, 이름, 퍼센트가 담긴 최상위 부모 오브젝트

    [Header("게이지 이미지 (Image Type이 Filled여야 함)")]
    public Image mainFillImage;   // 실제 에너지 바 이미지
    public Image spectrumFillImage;  // 따라오는 잔상 이미지

    [Header("텍스트")]
    public TextMeshProUGUI percentageText;

    [Header("패턴 4 전멸기 타이머 UI")]
    public Image timerBarImage; // 전멸기 게이지용 Image (Filled)
    public GameObject timerGroup; // 타이머 바와 배경을 묶은 부모 오브젝트

    [Header("설정")]
    public float lerpSpeed = 5f;

    private void Start()
    {
        // 시작할 때 게이지를 현재 에너지 비율로 초기화
        if (bossHealth != null)
        {
            float fillRatio = (float)bossHealth.currentHP / bossHealth.maxHP;
            mainFillImage.fillAmount = fillRatio;
            spectrumFillImage.fillAmount = fillRatio;
        }
    }

    private void Update()
    {
        if (bossHealth == null) return;

        // 1. 목표 비율 계산 (0.0 ~ 1.0)
        float targetFill = (float)bossHealth.currentHP / bossHealth.maxHP;

        // 2. 실제 게이지: 빠르게 변화 (타격감)
        mainFillImage.fillAmount = Mathf.Lerp(mainFillImage.fillAmount, targetFill, Time.deltaTime * lerpSpeed * 2f);

        // 3. 잔상 게이지: 부드럽게 추격 (시각적 잔상)
        spectrumFillImage.fillAmount = Mathf.Lerp(spectrumFillImage.fillAmount, targetFill, Time.deltaTime * lerpSpeed);

        // 4. 퍼센트 텍스트 업데이트 (0~100%)
        if (percentageText != null)
        {
            percentageText.text = $"{(targetFill * 100f):F0}%"; // 소수점 없이 정수로 표시
        }

        // 5. Shutdown 시 UI 처리
        if (bossHealth.currentStatus == BossHealth.BossState.Defeated)
        {
            // 여기서 UI를 서서히 끄거나 노이즈 연출을 할 수 있습니다.
        }
        // 패턴 4 타이머 UI 업데이트
        if (bossHealth != null && timerBarImage != null)
        {
            // 보스 컨트롤러 참조 (없으면 찾기)
            BossController controller = bossHealth.GetComponent<BossController>();

            if (controller != null && controller.pattern4Elapsed > 0)
            {
                if (timerGroup != null) timerGroup.SetActive(true); // 패턴 시작 시 노출

                // 비율 계산 (0 ~ 1)
                float ratio = controller.pattern4Elapsed / controller.timeLimit;
                timerBarImage.fillAmount = ratio;
            }
            else
            {
                if (timerGroup != null) timerGroup.SetActive(false); // 패턴 종료/그로기 시 숨김
            }
        }
    }
    public void SetVisible(bool visible)
    {
        if (hpCanvasRoot != null)
        {
            hpCanvasRoot.SetActive(visible);
        }
    }
}