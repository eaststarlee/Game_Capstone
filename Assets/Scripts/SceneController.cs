using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    [Header("UI 연결")]
    public GameObject clearMessageUI;  // 클리어 메시지 Text
    public GameObject loadingScreenUI; // 로딩 화면 (Now Loading)

    public void ClearBossAndLoadNextStage()
    {
        StartCoroutine(LoadNextStageRoutine());
    }

    IEnumerator LoadNextStageRoutine()
    {
        // 1. Text On
        if (clearMessageUI != null) clearMessageUI.SetActive(true);

        // 2. 3초 대기
        yield return new WaitForSeconds(3f);

        // 💾 1번 Auto Save 슬롯에 Stage2로 지정 저장!
        SaveManager.SaveGame(1, "Stage2");

        // 3. Turn on Loading
        if (clearMessageUI != null) clearMessageUI.SetActive(false);
        if (loadingScreenUI != null) loadingScreenUI.SetActive(true);

        // 4. Standby
        yield return new WaitForSeconds(0.5f);

        // 5. Stage2 Starts
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Stage2");

        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }
}