using System;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public InventorySlot[] slots = new InventorySlot[7];

    // 현재 선택된 슬롯 인덱스
    public int selectedIndex { get; private set; } = 0;

    // 데이터 변경 이벤트
    public event Action OnInventoryChanged;

    private void Update()
    {
        HandleNumberKeyInput();
    }

    // 숫자키(1~7)로 슬롯 선택
    private void HandleNumberKeyInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectSlot(0);
        else if (Input.GetKeyDown(KeyCode.Alpha2)) SelectSlot(1);
        else if (Input.GetKeyDown(KeyCode.Alpha3)) SelectSlot(2);
        else if (Input.GetKeyDown(KeyCode.Alpha4)) SelectSlot(3);
        else if (Input.GetKeyDown(KeyCode.Alpha5)) SelectSlot(4);
        else if (Input.GetKeyDown(KeyCode.Alpha6)) SelectSlot(5);
        else if (Input.GetKeyDown(KeyCode.Alpha7)) SelectSlot(6);
    }

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

    // ==========================================
    // 💾 세이브 / 로드 연동용 메서드
    // ==========================================

    /// <summary>
    /// 1. [Save용] 현재 7개 슬롯의 아이템 이름 목록을 순서대로 추출
    /// </summary>
    public System.Collections.Generic.List<string> GetSaveData()
    {
        System.Collections.Generic.List<string> itemNames = new System.Collections.Generic.List<string>();

        foreach (var slot in slots)
        {
            // 빈 슬롯이면 빈 문자열(""), 아이템이 있으면 itemName 저장
            itemNames.Add(slot.HasItem ? slot.itemName : "");
        }

        return itemNames;
    }

    /// <summary>
    /// 2. [Load용] 저장된 아이템 이름 목록을 바탕으로 슬롯 데이터 복원
    /// </summary>
    public void LoadSaveData(System.Collections.Generic.List<string> savedItemNames)
    {
        if (savedItemNames == null) return;

        // 인벤토리 슬롯 전체 초기화
        ClearAllSlots();

        // 저장된 개수만큼 슬롯 채우기 (최대 슬롯 크기 제한)
        int count = Mathf.Min(savedItemNames.Count, slots.Length);
        for (int i = 0; i < count; i++)
        {
            if (!string.IsNullOrEmpty(savedItemNames[i]))
            {
                slots[i].itemName = savedItemNames[i];

                // 💡 [참고] 만약 아이템 이름에 맞는 Sprite 아이콘도 불러와야 한다면 
                // 데이터베이스나 Resource.Load를 통해 slots[i].itemIcon을 할당하는 로직을 여기에 추가하시면 됩니다.
            }
        }

        // UI 갱신 이벤트 호출
        OnInventoryChanged?.Invoke();
    }

    /// <summary>
    /// 3. 인벤토리 전체 비우기
    /// </summary>
    public void ClearAllSlots()
    {
        foreach (var slot in slots)
        {
            slot.Clear();
        }
        OnInventoryChanged?.Invoke();
    }
}
