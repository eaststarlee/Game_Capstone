using UnityEngine;

public class Inventory : MonoBehaviour
{
    public InventorySlot[] slots = new InventorySlot[7];

    // 현재 선택된 슬롯 인덱스
    public int selectedIndex { get; private set; } = 0;

    // 슬롯 선택 변경
    public void SelectSlot(int index)
    {
        if (index < 0) index = 0;
        if (index >= slots.Length) index = slots.Length - 1;

        selectedIndex = index;
    }

    // 현재 선택된 슬롯 가져오기
    public InventorySlot GetSelectedSlot()
    {
        return slots[selectedIndex];
    }

    // 특정 인덱스 슬롯 가져오기
    public InventorySlot GetSlot(int index)
    {
        if (index < 0 || index >= slots.Length) return null;
        return slots[index];
    }

    public void AddItemToSlot(int slotIndex, string itemName, int amount)
    {
        if (slotIndex < 0 || slotIndex >= slots.Length) return;

        InventorySlot slot = slots[slotIndex];

        // 1. 현재 슬롯이 비었거나 같은 아이템이면 합산
        if (slot.IsEmpty || slot.itemName == itemName)
        {
            int total = slot.amount + amount;
            slot.itemName = itemName;
            slot.amount = Mathf.Min(total, 5); // 최대 5개 제한
            amount = total - slot.amount;       // 남은 수량 계산
        }
        else
        {
            // 2. 다른 아이템이면 남은 수량 그대로 처리
            // 현재 슬롯에는 손대지 않고 남은 amount 그대로 다음 단계로
        }

        // 3. 남은 수량이 있으면 다른 슬롯으로 자동 분배
        if (amount > 0)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (i == slotIndex) continue; // 현재 슬롯 건너뜀

                InventorySlot s = slots[i];

                if (s.IsEmpty || s.itemName == itemName)
                {
                    int total = s.amount + amount;
                    s.itemName = itemName;
                    s.amount = Mathf.Min(total, 5);
                    amount = total - s.amount;

                    if (amount <= 0)
                        break; // 남은 수량 모두 분배 완료
                }
            }
        }

        // 4. 만약 슬롯이 모두 찼으면 남은 수량은 버려짐(또는 처리 정책에 따라 다르게 설정)
    }

}
