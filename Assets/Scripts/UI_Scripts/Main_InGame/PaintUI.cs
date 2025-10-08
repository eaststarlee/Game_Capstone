using UnityEngine;
using UnityEngine.UI;

public class PaintUI : MonoBehaviour
{
    public Inventory inventory; // 인벤토리 참조

    [System.Serializable]
    public class ItemAmountUI
    {
        public string itemName;        // 아이템 이름
        public Image[] amountImages;   // 수량별 이미지 배열, [0] = 1개, [4] = 5개
    }

    public ItemAmountUI[] itemsUI; // 모든 아이템 UI 매핑

    private string previousItemName = null;
    private int previousAmount = 0;

    void Update()
    {
        InventorySlot currentSlot = inventory.GetSelectedSlot();

        string currentItemName = (currentSlot != null && !currentSlot.IsEmpty) ? currentSlot.itemName : null;
        int currentAmount = (currentSlot != null) ? currentSlot.amount : 0;

        // 이전 이미지 비활성화
        if (!string.IsNullOrEmpty(previousItemName))
        {
            ItemAmountUI prevUI = GetItemUI(previousItemName);
            if (prevUI != null && previousAmount > 0 && previousAmount <= prevUI.amountImages.Length)
            {
                prevUI.amountImages[previousAmount - 1].gameObject.SetActive(false);
            }
        }

        // 현재 이미지 활성화
        if (!string.IsNullOrEmpty(currentItemName) && currentAmount > 0)
        {
            ItemAmountUI currentUI = GetItemUI(currentItemName);
            if (currentUI != null && currentAmount <= currentUI.amountImages.Length)
            {
                currentUI.amountImages[currentAmount - 1].gameObject.SetActive(true);
            }
        }

        previousItemName = currentItemName;
        previousAmount = currentAmount;
    }

    private ItemAmountUI GetItemUI(string itemName)
    {
        foreach (var ui in itemsUI)
        {
            if (ui.itemName == itemName)
                return ui;
        }
        return null;
    }
}
