using UnityEngine;

public static class GameLoader
{
    public static bool hasLoadedPosition = false;
    public static Vector3 loadedPlayerPosition = Vector3.zero;

    // ⚡ [추가 권장] 로드한 SaveData 통째로 보관
    public static SaveData CurrentLoadedData { get; private set; }

    public static void SetLoadData(SaveData data)
    {
        if (data == null) return;

        CurrentLoadedData = data;
        loadedPlayerPosition = new Vector3(data.posX, data.posY, data.posZ);
        hasLoadedPosition = true;

        Debug.Log($"[GameLoader] 이동할 목표 좌표 및 세이브 데이터 저장 완료: {loadedPlayerPosition}");
    }

    public static void ClearData()
    {
        hasLoadedPosition = false;
        CurrentLoadedData = null;
    }
}