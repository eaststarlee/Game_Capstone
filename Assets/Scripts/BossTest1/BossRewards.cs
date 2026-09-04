using UnityEngine;

public class BossReward : MonoBehaviour
{
    [Header("보상 아이템 설정")]
    public string itemName = "YELLOW"; // 노란색 잉크 이름
    public Sprite itemIcon;            // 노란색 잉크 이미지

    private Inventory uiInventory;
    private bool hasGivenReward = false;

    private void Start()
    {
        GameObject manager = GameObject.Find("InventoryManager");
        if (manager != null)
        {
            uiInventory = manager.GetComponent<Inventory>();
            if (uiInventory == null)
                Debug.LogError("BossReward: InventoryManager에 Inventory.cs가 없습니다!");
        }
    }

    public void GiveYellowInk()
    {
        if (hasGivenReward) return; // 중복 지급 방지
        if (uiInventory == null) return;

        // 인벤토리에 추가
        int slotIndex = uiInventory.selectedIndex;
        uiInventory.AddItemToSlot(slotIndex, itemName, itemIcon);

        Debug.Log($"[BossReward] 보스 처치! {itemName} 자동 획득 완료");
        hasGivenReward = true;
    }
}