using UnityEngine;

public class ExitGame : MonoBehaviour
{
    // 버튼 클릭 시 호출
    public void QuitGame()
    {
        // 에디터에서는 종료 안 되도록 로그 출력
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit(); // 빌드된 게임 종료
#endif
    }
}
