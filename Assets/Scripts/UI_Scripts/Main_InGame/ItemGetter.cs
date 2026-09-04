using UnityEngine;
using System.Collections;

public class ItemGetter : MonoBehaviour
{
    [Header("아이템 설정")]
    public string itemName;
    public Sprite itemIcon;       // 아이템 아이콘 (선택 사항)
    public string playerTag = "Player";

    private bool hasGivenItem = false;
    private Inventory uiInventory;

    private void Start()
    {
        // 한 프레임 기다린 후 InventoryManager 탐색
        StartCoroutine(AssignInventoryNextFrame());
    }

    private IEnumerator AssignInventoryNextFrame()
    {
        yield return null;

        GameObject manager = GameObject.Find("InventoryManager");
        if (manager != null)
        {
            uiInventory = manager.GetComponent<Inventory>();
            if (uiInventory == null)
                Debug.LogError("ItemGetter: InventoryManager에 Inventory.cs가 없습니다!");
        }
        else
        {
            Debug.LogError("ItemGetter: InventoryManager 오브젝트를 찾을 수 없습니다!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasGivenItem) return;
        if (!other.CompareTag(playerTag)) return;
        if (uiInventory == null) return;

        Debug.Log($"{other.name}에 트리거됨, {itemName} 지급");

        // 선택된 슬롯에 아이템 추가
        int slotIndex = uiInventory.selectedIndex;
        uiInventory.AddItemToSlot(slotIndex, itemName, itemIcon);

        hasGivenItem = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        hasGivenItem = false; // 다시 접촉 시 아이템 지급 가능
    }
}
