using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SaveManager
{
    private static string GetPath(int slotNumber) => Path.Combine(Application.persistentDataPath, $"save_{slotNumber}.json");
    private static string GetImgPath(int slotNumber) => Path.Combine(Application.persistentDataPath, $"save_{slotNumber}.png");

    // ⚡ 1번 슬롯 전용 AutoSave
    public static void AutoSave(Vector3 playerPosition, Texture2D screenshot = null)
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        SaveGame(1, currentSceneName, playerPosition, screenshot);
        Debug.Log("[AutoSave] 1번 슬롯 자동저장 완료!");
    }

    // 기존 호출 호환용 오버로딩
    public static void SaveGame(int slotNumber, string sceneNameToSave)
    {
        SaveGame(slotNumber, sceneNameToSave, Vector3.zero, null);
    }

    // 세이브 메인 함수
    public static void SaveGame(int slotNumber, string sceneNameToSave, Vector3 position = default, Texture2D screenshot = null)
    {
        SaveData data = new SaveData
        {
            savedSceneName = sceneNameToSave,
            saveTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
            playTime = GetFormattedPlayTime(),
            posX = position.x,
            posY = position.y,
            posZ = position.z
        };

        // 1) 인벤토리 데이터 수집
        Inventory inventory = Object.FindFirstObjectByType<Inventory>();
        if (inventory != null)
        {
            data.inventoryItemIDs = inventory.GetSaveData();
        }

        // 2) 가이드북 UI 해금 목록 수집
        if (StringActivator.Instance != null)
        {
            data.unlockedGuidebookIDs = StringActivator.Instance.GetActivatedUINames();
        }

        string jsonText = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetPath(slotNumber), jsonText);

        // 스크린샷 이미지 PNG 저장
        if (screenshot != null)
        {
            byte[] bytes = screenshot.EncodeToPNG();
            File.WriteAllBytes(GetImgPath(slotNumber), bytes);
        }
    }

    // 세이브 데이터 읽기
    public static SaveData LoadGame(int slotNumber)
    {
        string path = GetPath(slotNumber);
        if (!File.Exists(path)) return null;
        return JsonUtility.FromJson<SaveData>(File.ReadAllText(path));
    }

    // 📸 RawImage용 Texture2D 스크린샷 불러오기 함수
    public static Texture2D LoadScreenshotTexture(int slotNumber)
    {
        string imgPath = GetImgPath(slotNumber);
        if (!File.Exists(imgPath)) return null;

        byte[] bytes = File.ReadAllBytes(imgPath);
        Texture2D texture = new Texture2D(2, 2);
        if (texture.LoadImage(bytes))
        {
            return texture;
        }
        return null;
    }

    // 슬롯 존재 여부 확인
    public static bool HasSaveData(int slotNumber) => File.Exists(GetPath(slotNumber));

    // 플레이 타임 포맷 변환
    private static string GetFormattedPlayTime()
    {
        int totalSeconds = (int)Time.time;
        int hours = totalSeconds / 3600;
        int minutes = (totalSeconds % 3600) / 60;
        int seconds = totalSeconds % 60;
        return $"{hours:D2}:{minutes:D2}:{seconds:D2}";
    }

    // ⚡ 불러온 SaveData를 게임 시스템 및 UI에 일괄 적용
    public static void ApplySaveData(SaveData data)
    {
        if (data == null) return;

        Debug.Log("[SaveManager] 세이브 데이터 적용(ApplySaveData) 시작!");

        // 1) 인벤토리 복원
        Inventory inventory = Object.FindFirstObjectByType<Inventory>();
        if (inventory != null && data.inventoryItemIDs != null)
        {
            inventory.LoadSaveData(data.inventoryItemIDs);
        }

        // 2) 가이드북 UI 장부 복원
        if (data.unlockedGuidebookIDs != null)
        {
            if (StringActivator.Instance != null)
            {
                StringActivator.Instance.RestoreActivatedUI(data.unlockedGuidebookIDs);
                Debug.Log($"[SaveManager] 가이드북 해금 장부 복원 완료! ({data.unlockedGuidebookIDs.Count}개)");
            }
            else
            {
                Debug.LogError("[SaveManager 오류] StringActivator.Instance가 null입니다!");
            }
        }

        // 3) 트리거 파괴
        if (data.unlockedGuidebookIDs != null)
        {
            GuideTrigger[] triggers = Object.FindObjectsByType<GuideTrigger>(FindObjectsSortMode.None);
            foreach (var trigger in triggers)
            {
                if (data.unlockedGuidebookIDs.Contains(trigger.ExtraUIName))
                {
                    Debug.Log($"[SaveManager] 해금된 트리거 파괴: {trigger.ExtraUIName}");
                    Object.Destroy(trigger.gameObject);
                }
            }
        }
    }
}