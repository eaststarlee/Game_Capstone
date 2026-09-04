using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    public void OnClickLoadSlot(int slotNumber)
    {
        SaveData data = SaveManager.LoadGame(slotNumber);

        if (data != null)
        {
            // ⚡ [핵심 추가] 저장된 좌표 데이터를 LoadPanelController static 변수에 등록!
            LoadPanelController.hasLoadedPosition = true;
            LoadPanelController.loadedPlayerPosition = new Vector3(data.posX, data.posY, data.posZ);

            Debug.Log($"{slotNumber}번 슬롯 불러오기 성공! 씬: {data.savedSceneName}, 위치: {LoadPanelController.loadedPlayerPosition}");

            // 씬 이동
            SceneManager.LoadScene(data.savedSceneName);
        }
        else
        {
            Debug.Log($"{slotNumber}번 슬롯은 빈 공간입니다.");
        }
    }
}