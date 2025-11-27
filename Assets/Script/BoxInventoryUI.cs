using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BoxInventoryUI : MonoBehaviour
{
    [System.Serializable]
    public class SlotWidget
    {
        public Image icon;
        public TMP_Text nameText;
        public Button takeOutButton;
    }

    [Header("Refs")]
    public PlayerInteractionSystem player;
    public GameObject panelRoot;
    public KeyCode toggleKey = KeyCode.Tab;   // ปุ่มเปิด/ปิด Inventory
    public SlotWidget[] slotsUI;

    void Start()
    {
        if (panelRoot) panelRoot.SetActive(false);

        // hook ปุ่ม Take Out
        for (int i = 0; i < slotsUI.Length; i++)
        {
            int idx = i;
            if (slotsUI[i].takeOutButton != null)
                slotsUI[i].takeOutButton.onClick.AddListener(() => OnClickTakeOut(idx));
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey) && panelRoot)
            panelRoot.SetActive(!panelRoot.activeSelf);

        if (panelRoot && panelRoot.activeSelf)
            Refresh();
    }

    void Refresh()
    {
        var inv = BoxInventory.Instance;
        if (inv == null) return;

        for (int i = 0; i < slotsUI.Length; i++)
        {
            var w = slotsUI[i];
            var slot = inv.GetSlot(i);

            if (slot != null && slot.hasBox && slot.itemData != null)
            {
                if (w.icon) w.icon.sprite = slot.itemData.icon;
                if (w.nameText) w.nameText.text = slot.itemData.itemName;
                if (w.takeOutButton) w.takeOutButton.interactable = (player != null && player.HeldObject == null);
            }
            else
            {
                if (w.icon) w.icon.sprite = null;
                if (w.nameText) w.nameText.text = "- EMPTY -";
                if (w.takeOutButton) w.takeOutButton.interactable = false;
            }
        }
    }

    void OnClickTakeOut(int index)
    {
        if (!player) return;

        player.TakeBoxFromInventorySlot(index);
    }
}
