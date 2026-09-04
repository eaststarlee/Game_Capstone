using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChangeTrigger : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] private string sceneName;

    [Header("Options")]
    [SerializeField] private bool destroyAfterTrigger = true;

    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;

        if (other.CompareTag("Player"))
        {
            triggered = true;

            // 씬 전환
            SceneManager.LoadScene(sceneName);

            // 트리거 제거 옵션
            if (destroyAfterTrigger)
            {
                Destroy(gameObject);
            }
        }
    }
}