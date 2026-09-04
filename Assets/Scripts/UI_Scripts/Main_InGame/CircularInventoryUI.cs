using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CircularInventoryUI : MonoBehaviour
{
    public GameObject slotPrefab;
    public Transform slotParent;
    public int slotCount = 7;
    public float radius = 200f;

    public Inventory inventory;

    [Header("Item Visuals")]
    public List<ItemVisual> itemVisuals = new List<ItemVisual>();

    private List<Image> slotImages = new List<Image>();
    private List<Image> slotBorders = new List<Image>();
    private List<Sprite> slotDefaultIcons = new List<Sprite>(); // 각 슬롯의 기본 아이콘

    [Header("Debug Text")]
    public Text selectedSlotText;

    [System.Serializable]
    public class ItemVisual
    {
        public string itemName;   // 아이템 이름
        public Color color = Color.white; // 아이템 색상
    }

    void Start()
    {
        // 색상 이름 기반 ItemVisual 초기화
        itemVisuals = new List<ItemVisual>()
        {
            new ItemVisual() { itemName = "RED", color = Color.red },
            new ItemVisual() { itemName = "ORANGE", color = new Color(1f, 0.5f, 0f) },
            new ItemVisual() { itemName = "YELLOW", color = Color.yellow },
            new ItemVisual() { itemName = "GREEN", color = Color.green },
            new ItemVisual() { itemName = "BLUE", color = Color.blue },
            new ItemVisual() { itemName = "DARKBLUE", color = new Color(0f, 0f, 0.5f) },
            new ItemVisual() { itemName = "PURPLE", color = new Color(0.5f, 0f, 0.5f) },
            new ItemVisual() { itemName = "BLACK", color = Color.black }
        };

        GenerateSlots();
        RefreshUI();

        // Inventory 이벤트 구독
        if (inventory != null)
        {
            inventory.OnInventoryChanged += RefreshUI;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            inventory.PrevSlot();
            RefreshUI();
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            inventory.NextSlot();
            RefreshUI();
        }
    }

    void GenerateSlots()
    {
        float angleStep = 360f / (slotCount * 2f);
        float startAngle = 90f;

        for (int i = 0; i < slotCount; i++)
        {
            GameObject slot = Instantiate(slotPrefab, slotParent);
            RectTransform rt = slot.GetComponent<RectTransform>();

            float angle = startAngle - (i * angleStep);
            float rad = angle * Mathf.Deg2Rad;
            Vector2 pos = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * radius;
            rt.anchoredPosition = pos;

            // 루트 Image 가져오기
            Image img = slot.GetComponent<Image>();
            if (img != null)
            {
                slotImages.Add(img);
                slotDefaultIcons.Add(img.sprite); // 기본 아이콘 저장
            }

            // Border 가져오기
            Image border = slot.transform.Find("Border")?.GetComponent<Image>();
            if (border != null)
            {
                border.enabled = false;
                slotBorders.Add(border);
            }
        }
    }

    public void RefreshUI()
    {
        for (int i = 0; i < slotCount; i++)
        {
            InventorySlot slot = inventory.slots[i];
            Image img = slotImages[i];

            if (slot == null || !slot.HasItem)
            {
                // 빈 슬롯이면 기본 아이콘 유지
                img.sprite = slotDefaultIcons[i];
                img.color = Color.white;
            }
            else
            {
                // 아이콘이 있으면 적용
                if (slot.itemIcon != null)
                {
                    img.sprite = slot.itemIcon;
                    img.color = Color.white; // 색상 초기화
                }
                else
                {
                    img.sprite = slotDefaultIcons[i];
                    ItemVisual visual = itemVisuals.Find(v => v.itemName == slot.itemName);
                    img.color = visual != null ? visual.color : Color.white;
                }
            }

            // Border 표시
            if (slotBorders[i] != null)
            {
                slotBorders[i].enabled = (i == inventory.selectedIndex);
            }
        }

        // 선택 슬롯 번호 표시
        if (selectedSlotText != null)
            selectedSlotText.text = "" + (inventory.selectedIndex + 1);
    }
}
