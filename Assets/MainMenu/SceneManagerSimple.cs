using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerSimple : MonoBehaviour
{
    // 인스펙터에서 이 함수를 선택하면 입력창이 나타납니다.
    public void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}