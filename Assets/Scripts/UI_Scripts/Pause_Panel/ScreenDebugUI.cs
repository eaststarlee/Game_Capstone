using UnityEngine;
using TMPro;

public class ScreenDebugUI : MonoBehaviour
{
    [Header("해상도 값을 출력할 TMP 텍스트")]
    public TextMeshProUGUI resolutionText;

    [Header("2560x1440일 때만 보이게 할 오브젝트")]
    public GameObject onlyFor2560x1440;

    [Header("조건을 체크할 UI 패널")]
    public GameObject targetUIPanel;

    private void Update()
    {
        // 특정 UI 패널이 켜진 순간만 체크
        if (targetUIPanel != null && targetUIPanel.activeInHierarchy)
        {
            int width = Screen.width;
            int height = Screen.height;

            if (resolutionText != null)
            {
                resolutionText.text = $"{width} x {height}";
            }

            if (onlyFor2560x1440 != null)
            {
                onlyFor2560x1440.SetActive(width == 2560 && height == 1440);
            }
        }
        else
        {
            // UI 패널 꺼지면 자동으로 같이 꺼지게
            if (onlyFor2560x1440 != null)
            {
                onlyFor2560x1440.SetActive(false);
            }
        }
    }
}
