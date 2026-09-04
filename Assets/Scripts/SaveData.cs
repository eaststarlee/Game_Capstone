using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public string savedSceneName; // 예: "Stage1", "Stage2"
    public string saveTime;       // 예: "2026-07-28 21:30"
    public string playTime;       // 예: "01:23:45" (필요 시 사용)

    public float posX;
    public float posY;
    public float posZ;

    // 1. 인벤토리 UI 7개 슬롯의 아이템 이름 목록
    public List<string> inventoryItemIDs = new List<string>();

    // 2. 해금된 가이드북 UI 패널 이름 목록 (트리거 파괴 판정에도 같이 사용)
    public List<string> unlockedGuidebookIDs = new List<string>();
}