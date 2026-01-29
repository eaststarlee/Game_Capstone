using UnityEngine;
using UnityEngine.SceneManagement;

public class UISceneActivator : MonoBehaviour
{
    [Header("불러올 UI 씬 이름")]
    public string uiSceneName = "UIScene"; // UI씬 이름

    void Start()
    {
        // UI씬이 이미 열려 있는지 확인 후 Additive 로드
        if (!SceneManager.GetSceneByName(uiSceneName).isLoaded)
        {
            SceneManager.LoadScene(uiSceneName, LoadSceneMode.Additive);
        }
    }
}
