using UnityEngine;
using UnityEngine.UI; // UI 컴포넌트 사용을 위해 필요

public class HealthUI : MonoBehaviour
{
    [Header("참조 설정")]
    private PlayerHealth playerHealth; // 인스펙터 드래그 대신 코드로 할당
    public Image[] heartImages;

    [Header("아이콘 설정")]
    public Sprite fullHeart;  // 채워진 하트 이미지
    public Sprite emptyHeart; // 비워진 하트 이미지 (선택 사항)
    void Start()
    {
        // 싱글톤 인스턴스를 찾아서 할당
        playerHealth = PlayerHealth.Instance;

        if (playerHealth == null)
        {
            Debug.LogError("PlayerHealth 인스턴스를 찾을 수 없습니다! Player 씬이 로드되었는지 확인하세요.");
        }
    }
    void Update()
    {
        // 매 프레임 혹은 체력이 변할 때마다 UI 업데이트
        UpdateHealthDisplay();
    }

    public void UpdateHealthDisplay()
    {
        for (int i = 0; i < heartImages.Length; i++)
        {
            // 현재 체력보다 인덱스가 작으면 '꽉 찬 하트', 크면 '빈 하트'
            if (i < playerHealth.currentHealth)
            {
                heartImages[i].sprite = fullHeart;
                heartImages[i].enabled = true; // 혹은 이미지를 교체하는 대신 끄고 켜기만 해도 됨
            }
            else
            {
                // 체력이 깎인 칸 처리
                if (emptyHeart != null)
                {
                    heartImages[i].sprite = emptyHeart;
                }
                else
                {
                    heartImages[i].enabled = false; // 빈 하트 이미지가 없다면 그냥 숨기기
                }
            }
        }
    }
}