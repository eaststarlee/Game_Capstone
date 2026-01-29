using System;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public InventorySlot[] slots = new InventorySlot[7];

    // 현재 선택된 슬롯 인덱스
    public int selectedIndex { get; private set; } = 0;

    // 데이터 변경 이벤트
    public event Action OnInventoryChanged;

    // 슬롯 선택 변경
    public void SelectSlot(int index)
    {
        if (index < 0) index = 0;
        if (index >= slots.Length) index = slots.Length - 1;
        selectedIndex = index;

        OnInventoryChanged?.Invoke(); // UI 갱신 트리거
    }

    // 선택된 슬롯 가져오기
    public InventorySlot GetSelectedSlot()
    {
        return slots[selectedIndex];
    }

    // 특정 슬롯 가져오기
    public InventorySlot GetSlot(int index)
    {
        if (index < 0 || index >= slots.Length) return null;
        return slots[index];
    }

    // 아이템 추가 (존재 여부 기반)
    public void AddItemToSlot(int slotIndex, string itemName, Sprite icon = null)
    {
        if (slotIndex < 0 || slotIndex >= slots.Length) return;

        // 인벤토리 전체를 검사하여 이미 같은 이름이 있는지 확인
        foreach (var slot in slots)
        {
            if (slot.HasItem && slot.itemName == itemName)
            {
                Debug.Log($"{itemName}은(는) 이미 인벤토리에 존재합니다.");
                return; // 이미 존재하면 추가하지 않음
            }
        }

        InventorySlot slotToAdd = slots[slotIndex];

        // 슬롯이 비어있으면 지정된 슬롯에 추가
        if (!slotToAdd.HasItem)
        {
            slotToAdd.itemName = itemName;
            slotToAdd.itemIcon = icon;
            OnInventoryChanged?.Invoke();
            return;
        }

        // 지정된 슬롯이 차 있으면 첫 번째 빈 슬롯 찾아 추가
        for (int i = 0; i < slots.Length; i++)
        {
            if (!slots[i].HasItem)
            {
                slots[i].itemName = itemName;
                slots[i].itemIcon = icon;
                OnInventoryChanged?.Invoke();
                return;
            }
        }

        // 모든 슬롯이 차있으면 실패
        Debug.Log($"모든 슬롯이 차있어 {itemName}을(를) 추가할 수 없습니다.");
    }


    // 선택된 슬롯 아이템 제거
    public void RemoveSelectedItem()
    {
        InventorySlot slot = GetSelectedSlot();
        if (slot.HasItem)
        {
            slot.Clear();
            OnInventoryChanged?.Invoke(); // 제거 시 UI 갱신
        }
    }

    // 다음 슬롯 선택
    public void NextSlot()
    {
        selectedIndex = (selectedIndex + 1) % slots.Length;
        OnInventoryChanged?.Invoke();
    }

    // 이전 슬롯 선택
    public void PrevSlot()
    {
        selectedIndex = (selectedIndex - 1 + slots.Length) % slots.Length;
        OnInventoryChanged?.Invoke();
    }
}
