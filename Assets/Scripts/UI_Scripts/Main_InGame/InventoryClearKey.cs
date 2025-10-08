using UnityEngine;

public class InventoryClearKey : MonoBehaviour
{
    [SerializeField] private KeyCode clearKey = KeyCode.C; // 인스펙터에서 키 선택 가능
    [SerializeField] private Inventory inventory; // 인벤토리 참조
    [SerializeField] private CircularInventoryUI circularInventoryUI; // UI 갱신을 위해 참조

    private void Update()
    {
        if (Input.GetKeyDown(clearKey))
        {
            InventorySlot selectedSlot = inventory.GetSelectedSlot();

            if (selectedSlot != null)
            {
                selectedSlot.amount = 0;     // 슬롯 수량 0으로 초기화
                selectedSlot.itemName = "";  // 아이템 이름 초기화
                Debug.Log("선택된 슬롯이 비워졌습니다.");

                // 바로 UI 갱신
                if (circularInventoryUI != null)
                    circularInventoryUI.RefreshUI();
            }
        }
    }
}
