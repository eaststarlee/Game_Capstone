using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    [Header("참조 설정")]
    private PlayerHealth playerHealth;

    public Image[] heartImages;
    void Start()
    {
        // PlayerHealth 싱글톤 인스턴스 찾기
        playerHealth = PlayerHealth.Instance;

        if (playerHealth == null)
        {
            Debug.LogError("[HealthUI] PlayerHealth를 찾을 수 없습니다!");
        }
    }

    void Update()
    {
        // 매 프레임 UI 갱신
        UpdateHealthDisplay();
    }

    public void UpdateHealthDisplay()
    {
        if (playerHealth == null) return;

        float regenProgress = playerHealth.GetRegenProgress();

        for (int i = 0; i < heartImages.Length; i++)
        {
            // 이미 꽉 찬 하트들
            if (i < playerHealth.currentHealth)
            {
                heartImages[i].fillAmount = 1f; 
            }
            // 지금 차오르고 있는 하트
            else if (i == playerHealth.currentHealth)
            {
                // 로딩 애니메이션 적용
                heartImages[i].fillAmount = regenProgress;
            }
            // 아직 비어있는 하트들
            else
            {
                heartImages[i].fillAmount = 0f;
            }
        }
    }
}