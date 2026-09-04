using UnityEngine;
using UnityEngine.SceneManagement;

public class SavePanelController : MonoBehaviour
{
    private static string cachedSceneName;
    private static Vector3 cachedPosition;
    private static Texture2D cachedScreenshot;

    [Header("Slot UI List")]
    public SaveSlotUI[] slotUIs;

    public static void SetCurrentSaveData(string sceneName, Vector3 position, Texture2D screenshot)
    {
        cachedSceneName = sceneName;
        cachedPosition = position;
        cachedScreenshot = screenshot;
    }

    private void OnEnable()
    {
        RefreshAllSlots();
    }

    public void RefreshAllSlots()
    {
        if (slotUIs == null) return;
        foreach (var slot in slotUIs)
        {
            if (slot != null) slot.RefreshUI();
        }
    }

    // 2, 3번 수동 저장 버튼 눌렀을 때
    public void OnClickSaveSlot(int slotNumber)
    {
        if (string.IsNullOrEmpty(cachedSceneName))
        {
            cachedSceneName = SceneManager.GetActiveScene().name;
        }

        SaveManager.SaveGame(slotNumber, cachedSceneName, cachedPosition, cachedScreenshot);
        Debug.Log($"[SavePanel] {slotNumber}번 슬롯 수동 저장 완료!");

        // 저장 후 화면 바로 갱신
        RefreshAllSlots();
    }

    public void OnClickCloseUI()
    {
        gameObject.SetActive(false);
    }
}