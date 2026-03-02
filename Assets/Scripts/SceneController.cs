using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    [Header("UI 연결하기")]
    public GameObject clearMessageUI;  // Text
    public GameObject loadingScreenUI; // 검은 화면 (Now Loading)

    public void ClearBossAndLoadNextStage()
    {
        StartCoroutine(LoadNextStageRoutine());
    }

    IEnumerator LoadNextStageRoutine()
    {
        // 1. Text On
        if (clearMessageUI != null) clearMessageUI.SetActive(true);

        // 2. Waiting for 3 Seconds
        yield return new WaitForSeconds(3f);

        // 3. Turn on Loading
        if (clearMessageUI != null) clearMessageUI.SetActive(false);
        if (loadingScreenUI != null) loadingScreenUI.SetActive(true);

        // 4. Standby
        yield return new WaitForSeconds(0.5f);

        // 5. Stage2 Starts
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Stage2");

        // Standby
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }
}