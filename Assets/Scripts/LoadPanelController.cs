using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadPanelController : MonoBehaviour
{
    // 💡 씬 전환 후 플레이어를 저장 위치로 복원하기 위한 static 변수
    public static bool hasLoadedPosition = false;
    public static Vector3 loadedPlayerPosition = Vector3.zero;

    [Header("Slot UI List")]
    public SaveSlotUI[] slotUIs;

    // Load 패널 창이 활성화될 때마다(OnEnable) 세이브 파일 정보 최신화
    private void OnEnable()
    {
        RefreshAllSlots();
    }

    // 슬롯 1, 2, 3 화면 갱신
    public void RefreshAllSlots()
    {
        if (slotUIs == null) return;
        foreach (var slot in slotUIs)
        {
            if (slot != null) slot.RefreshUI();
        }
    }

    // 불러오기 버튼 클릭 시 호출
    public void OnClickLoadSlot(int slotNumber)
    {
        SaveData data = SaveManager.LoadGame(slotNumber);

        if (data != null)
        {
            // ⚡ [수정] 좌표만 넘기지 않고 SaveData 객체 전체를 GameLoader에 전달
            GameLoader.SetLoadData(data);

            Debug.Log($"[{slotNumber}번 슬롯 불러오기 성공] 씬: {data.savedSceneName}, 위치: ({data.posX}, {data.posY}, {data.posZ})");

            // 2. 씬 이동
            SceneManager.LoadScene(data.savedSceneName);
        }
        else
        {
            Debug.Log($"[{slotNumber}번 슬롯] 비어있는 슬롯입니다.");
        }
    }
}