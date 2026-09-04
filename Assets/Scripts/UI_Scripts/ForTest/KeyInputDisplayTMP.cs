using UnityEngine;
using TMPro;

public class KeyInputDisplayTMP : MonoBehaviour
{
    public TextMeshProUGUI keyDisplayText; // TMP Text 참조
    private float displayTimer = 0f;       // 표시 유지 시간
    private string lastPressedKeys = "";   // 마지막 입력 키 문자열

    void Update()
    {
        // 현재 눌린 키 탐색 (한 프레임에서 눌린 키 조합)
        string pressedKeys = "";

        foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(key)) // 키를 누른 순간만 감지
            {
                if (pressedKeys.Length > 0)
                    pressedKeys += " + ";
                pressedKeys += key.ToString();
            }
        }

        // 새로운 키 입력이 있었을 때만 업데이트
        if (!string.IsNullOrEmpty(pressedKeys))
        {
            lastPressedKeys = pressedKeys;
            keyDisplayText.text = "현재 입력된 키 : " + lastPressedKeys;
            displayTimer = 5f; // 5초 유지
        }

        // 타이머 감소
        if (displayTimer > 0f)
        {
            displayTimer -= Time.deltaTime;

            if (displayTimer <= 0f)
            {
                keyDisplayText.text = "현재 입력된 키 : 없음";
                lastPressedKeys = "";
            }
        }
    }
}
