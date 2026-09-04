using UnityEngine;

[System.Serializable]
public class InventorySlot
{
    public string itemName;  // 슬롯에 들어있는 아이템 이름

    // 아이템 존재 여부
    public bool HasItem => !string.IsNullOrEmpty(itemName);

    // 슬롯 초기화
    public void Clear()
    {
        itemName = null;
    }

    // UI용: 슬롯에 표시할 아이콘 반환 (참고용, 필요시 연결)
    public Sprite itemIcon; // 아이템 하나당 하나의 아이콘
    public Sprite GetSprite() => HasItem ? itemIcon : null;
}
