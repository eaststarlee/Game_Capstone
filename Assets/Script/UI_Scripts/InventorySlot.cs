using UnityEngine;

[System.Serializable]
public class InventorySlot
{
    public string itemName;
    public int amount;

    // 수량별 이미지 배열 (0~max 수량용)
    public Sprite[] amountSprites;

    public bool IsEmpty => amount <= 0;

    // 현재 수량에 맞는 이미지 반환
    public Sprite GetSpriteForAmount()
    {
        if (IsEmpty || amountSprites == null || amountSprites.Length == 0)
            return null;

        int index = Mathf.Clamp(amount - 1, 0, amountSprites.Length - 1);
        return amountSprites[index];
    }
}
