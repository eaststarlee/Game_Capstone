using UnityEngine;

public class UseItemButton : MonoBehaviour
{
    public Inventory inventory;                  // 인벤토리 참조
    public CircularInventoryUI circularInventoryUI; // UI 갱신용 참조

    // 버튼 클릭 시 호출
    public void OnUseItem()
    {
        InventorySlot slot = inventory.GetSelectedSlot(); // 현재 선택 슬롯 가져오기

        if (slot == null || slot.IsEmpty)
        {
            Debug.Log("슬롯이 비어있습니다.");
            return;
        }

        // 아이템 수량 감소
        slot.amount -= 1;

        // 수량이 0이면 슬롯 비었음 처리
        if (slot.amount <= 0)
        {
            slot.amount = 0;
            slot.itemName = "";  // ← 여기를 추가해서 아이템명 초기화
            Debug.Log("슬롯이 비어있습니다.");
        }

        // 수량 변경 후 바로 UI 갱신
        if (circularInventoryUI != null)
        {
            circularInventoryUI.RefreshUI();
        }
    }
}
