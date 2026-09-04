using UnityEngine;

public class CursorToggle : MonoBehaviour
{
    void Start()
    {
        // 게임 시작 시 커서를 숨기고 잠금
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // 좌측 Alt 키 홀딩 시 커서 보이기, 떼면 숨기기
        if (Input.GetKey(KeyCode.LeftAlt))
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None; // 커서 이동 가능
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked; // 커서 화면 중앙 고정
        }
    }
}
