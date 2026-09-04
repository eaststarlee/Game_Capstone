using UnityEngine;

public class UseItemButton : MonoBehaviour
{
    public Inventory inventory;                    // 인벤토리 참조
    public CircularInventoryUI circularInventoryUI; // UI 갱신용 참조

    // 버튼 클릭 시 호출
    public void OnUseItem()
    {
        InventorySlot slot = inventory.GetSelectedSlot(); // 현재 선택 슬롯 가져오기

        if (slot == null || !slot.HasItem)  // IsEmpty → !HasItem
        {
            Debug.Log("슬롯이 비어있습니다.");
            return;
        }

        // 아이템 사용 시 슬롯 비우기
        slot.Clear();
        Debug.Log("아이템을 사용했습니다. 슬롯이 비워졌습니다.");

        // UI 갱신
        if (circularInventoryUI != null)
        {
            circularInventoryUI.RefreshUI();
        }
    }
}
