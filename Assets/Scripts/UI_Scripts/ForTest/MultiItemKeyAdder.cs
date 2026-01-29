using UnityEngine;

[System.Serializable]
public class KeyItem
{
    public KeyCode key;       // 아이템 추가 키
    public string itemName;   // 아이템 이름
}

public class MultiItemKeyAdder : MonoBehaviour
{
    [Header("아이템 키 설정")]
    public KeyItem[] keyItems;    // 키-아이템 매핑 리스트

    private Inventory uiInventory;

    private void Start()
    {
        // InventoryManager 찾기
        GameObject manager = GameObject.Find("InventoryManager");
        if (manager != null)
        {
            uiInventory = manager.GetComponent<Inventory>();
            if (uiInventory == null)
                Debug.LogError("MultiItemKeyAdder: InventoryManager에 Inventory.cs가 없습니다!");
        }
        else
        {
            Debug.LogError("MultiItemKeyAdder: InventoryManager 오브젝트를 찾을 수 없습니다!");
        }
    }

    private void Update()
    {
        if (uiInventory == null) return;

        // 키별 아이템 추가 처리
        foreach (var keyItem in keyItems)
        {
            if (Input.GetKeyDown(keyItem.key))
            {
                int slotIndex = uiInventory.selectedIndex;
                uiInventory.AddItemToSlot(slotIndex, keyItem.itemName);
                Debug.Log($"{keyItem.itemName}을(를) 슬롯 {slotIndex}에 추가 시도");
            }
        }
    }
}
