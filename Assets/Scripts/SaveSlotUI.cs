using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SaveSlotUI : MonoBehaviour
{
    public int slotNumber = 1;

    [Header("1. 세이브 데이터가 있을 때 보여줄 요소들")]
    [Tooltip("ScreenShot, TextGroup 등을 하나로 묶은 오브젝트 (선택사항)")]
    public GameObject dataContentGroup;
    public RawImage screenshotImage;
    public TMP_Text stageNameText;
    public TMP_Text saveDateText;
    public TMP_Text playTimeText;

    [Header("2. 빈 슬롯일 때 표시할 UI")]
    [Tooltip("중앙에 배치한 'Empty Slot' 텍스트 오브젝트")]
    public GameObject emptySlotGroup;

    // 슬롯 UI 화면 갱신
    public void RefreshUI()
    {
        bool hasData = SaveManager.HasSaveData(slotNumber);

        // 🟢 세이브 데이터가 존재하는 경우
        if (hasData)
        {
            SaveData data = SaveManager.LoadGame(slotNumber);

            // UI 켜기/끄기 전환
            if (dataContentGroup != null) dataContentGroup.SetActive(true);
            if (emptySlotGroup != null) emptySlotGroup.SetActive(false);

            // Data 텍스트 채우기
            if (stageNameText != null) stageNameText.text = data.savedSceneName;
            if (saveDateText != null) saveDateText.text = data.saveTime;
            if (playTimeText != null) playTimeText.text = data.playTime;

            // 스크린샷 텍스처 연결
            Texture2D capturedTex = SaveManager.LoadScreenshotTexture(slotNumber);
            if (screenshotImage != null)
            {
                screenshotImage.texture = capturedTex;
                screenshotImage.enabled = (capturedTex != null);
            }
        }
        // 🔴 세이브 데이터가 없는 경우 (빈 슬롯)
        else
        {
            // UI 켜기/끄기 전환
            if (dataContentGroup != null) dataContentGroup.SetActive(false);
            if (emptySlotGroup != null) emptySlotGroup.SetActive(true);

            // dataContentGroup을 따로 안 지정했을 때를 대비한 개별 비활성화 예비 로직
            if (dataContentGroup == null)
            {
                if (stageNameText != null) stageNameText.text = "";
                if (saveDateText != null) saveDateText.text = "";
                if (playTimeText != null) playTimeText.text = "";
                if (screenshotImage != null) screenshotImage.enabled = false;
            }
        }
    }
}