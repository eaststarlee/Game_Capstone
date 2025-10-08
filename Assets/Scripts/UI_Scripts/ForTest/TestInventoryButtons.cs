using UnityEngine;
using UnityEngine.UI;

public class TestInventoryButtons : MonoBehaviour
{
    public Inventory inventory;  // 연결된 Inventory
    public Button addREDButton;   
    public Button addORANGEButton;
    public Button addYELLOWButton;
    public Button addGREENButton;
    public Button addBLUEButton;
    public Button addDARKBLUEButton;
    public Button addPURPLEButton;
    public CircularInventoryUI circularInventoryUI; // 인스펙터에 드래그 앤 드롭

    void Start()
    {
        // 버튼 클릭 이벤트 등록
        addREDButton.onClick.AddListener(() => AddItem("RED", 5));
        addORANGEButton.onClick.AddListener(() => AddItem("ORANGE", 5));
        addYELLOWButton.onClick.AddListener(() => AddItem("YELLOW", 5));
        addGREENButton.onClick.AddListener(() => AddItem("GREEN", 5));
        addBLUEButton.onClick.AddListener(() => AddItem("BLUE", 5));
        addDARKBLUEButton.onClick.AddListener(() => AddItem("DARKBLUE", 5));
        addPURPLEButton.onClick.AddListener(() => AddItem("PURPLE", 5));
    }

    void AddItem(string itemName, int amount)
    {
        // 현재 선택 슬롯에 추가
        int selected = inventory.selectedIndex;
        inventory.AddItemToSlot(selected, itemName, amount);
        circularInventoryUI.RefreshUI();

        Debug.Log($"Added {amount}x {itemName} to slot {selected + 1}");
    }
}
