using UnityEngine;
using UnityEngine.UI;

public class PaintUI : MonoBehaviour
{
    public Inventory inventory; // 인벤토리 참조

    [System.Serializable]
    public class ItemUI
    {
        public string itemName;  // 아이템 이름
        public Image itemImage;  // 해당 아이템의 이미지 (수량 상관없이 하나)
    }

    public ItemUI[] itemsUI; // 모든 아이템 UI 매핑

    private string previousItemName = null;

    void Update()
    {
        InventorySlot currentSlot = inventory.GetSelectedSlot();
        string currentItemName = (currentSlot != null && currentSlot.HasItem) ? currentSlot.itemName : null;

        // 이전 아이템 이미지 비활성화
        if (!string.IsNullOrEmpty(previousItemName))
        {
            ItemUI prevUI = GetItemUI(previousItemName);
            if (prevUI != null)
                prevUI.itemImage.gameObject.SetActive(false);
        }

        // 현재 아이템 이미지 활성화
        if (!string.IsNullOrEmpty(currentItemName))
        {
            ItemUI currentUI = GetItemUI(currentItemName);
            if (currentUI != null)
                currentUI.itemImage.gameObject.SetActive(true);
        }

        previousItemName = currentItemName;
    }

    private ItemUI GetItemUI(string itemName)
    {
        foreach (var ui in itemsUI)
        {
            if (ui.itemName == itemName)
                return ui;
        }
        return null;
    }
}
