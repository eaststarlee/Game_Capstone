using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChangerKey : MonoBehaviour
{
    [SerializeField] private string targetSceneName; // 이동할 씬 이름
    [SerializeField] private KeyCode triggerKey = KeyCode.F; // 눌렀을 때 이동할 키

    private void Update()
    {
        if (Input.GetKeyDown(triggerKey))
        {
            SceneManager.LoadScene(targetSceneName);
        }
    }
}