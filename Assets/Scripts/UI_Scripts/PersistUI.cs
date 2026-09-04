using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistUI : MonoBehaviour
{
    private static PersistUI instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (instance == this)
            instance = null;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 메인 메뉴 씬 이름이 "MainMenu"인 경우
        if (scene.name == "EndingScene")
        {
            Destroy(gameObject);
        }
    }
}