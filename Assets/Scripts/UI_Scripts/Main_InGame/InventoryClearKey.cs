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

            if (selectedSlot != null && selectedSlot.HasItem)
            {
                selectedSlot.Clear();  // 슬롯 비우기
                Debug.Log("선택된 슬롯이 비워졌습니다.");

                // UI 바로 갱신
                if (circularInventoryUI != null)
                    circularInventoryUI.RefreshUI();
            }
        }
    }
}
