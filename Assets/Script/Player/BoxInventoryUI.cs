using NUnit;
using UnityEngine;

public class BoxInventoryUI : MonoBehaviour
{
    [Header("Root Panel")]
    public GameObject rootPanel;

    [Header("Slot UIs (สูงสุด 3 ช่อง)")]
    public BoxInventorySlotUI[] slotUIs;

    [Header("Toggle Key")]
    public KeyCode toggleKey = KeyCode.Alpha1;

    void Start()
    {
        if (rootPanel != null)
            rootPanel.SetActive(false);
    }

    void Update()
    {
        // Toggle UI
        if (Input.GetKeyDown(toggleKey))
        {
            if (rootPanel != null)
                rootPanel.SetActive(!rootPanel.activeSelf);
        }

        if (rootPanel == null || !rootPanel.activeSelf) return;

        var inv = BoxInventory.Instance;
        if (inv == null || slotUIs == null) return;

        // จำนวนช่องจริงจาก inventory (ตาม maxSlots / slots.Length)
        int realSlotCount = inv.SlotCount;

        for (int i = 0; i < slotUIs.Length; i++)
        {
            var ui = slotUIs[i];
            if (ui == null) continue;

            if (i < realSlotCount)
            {
                // เปิดเฉพาะช่องที่ inventory มีจริง
                ui.gameObject.SetActive(true);

                var slot = inv.GetSlot(i);
                ui.Refresh(slot, i);
            }
            else
            {
                // เกินจากจำนวน slot จริง → ซ่อน
                ui.gameObject.SetActive(false);
            }
        }
    }

}
