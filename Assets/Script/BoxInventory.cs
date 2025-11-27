using System;
using UnityEngine;

public class BoxInventory : MonoBehaviour
{
    public static BoxInventory Instance { get; private set; }

    [Header("Settings")]
    public int maxSlots = 3;

    [Header("Box Prefab (world object)")]
    public GameObject boxPrefab;   // พรีแฟบกล่องที่มี BoxCore + DeliveryItemInstance

    [Serializable]
    public class BoxSlot
    {
        public bool hasBox;
        public BoxKind boxType;
        public DeliveryItemData itemData;
        public float itemQuality;
    }

    public BoxSlot[] slots;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (slots == null || slots.Length != maxSlots)
            slots = new BoxSlot[maxSlots];

        for (int i = 0; i < slots.Length; i++)
            if (slots[i] == null) slots[i] = new BoxSlot();
    }

    public BoxSlot GetSlot(int idx)
    {
        if (idx < 0 || idx >= slots.Length) return null;
        return slots[idx];
    }

    int FindFirstFreeSlot()
    {
        for (int i = 0; i < slots.Length; i++)
            if (!slots[i].hasBox) return i;
        return -1;
    }

    public bool StoreBox(BoxCore box)
    {
        if (!box || !box.CurrentItemData || !box.CurrentItemInstance)
        {
            Debug.LogWarning("[BoxInventory] Missing box or item data");
            return false;
        }

        int free = FindFirstFreeSlot();
        if (free < 0)
        {
            Debug.Log("[BoxInventory] Inventory full");
            return false;
        }

        var slot = slots[free];
        slot.hasBox = true;
        slot.boxType = box.boxType;
        slot.itemData = box.CurrentItemData;
        slot.itemQuality = box.CurrentItemInstance.currentQuality;

        // ลบกล่องจากโลก (ถือว่าเก็บเข้าตัวจริง ๆ)
        Destroy(box.gameObject);

        Debug.Log($"[BoxInventory] Stored box in slot {free}");
        return true;
    }

    public BoxCore SpawnBoxFromSlot(int slotIndex, Transform spawnPoint)
    {
        if (slotIndex < 0 || slotIndex >= slots.Length) return null;

        var slot = slots[slotIndex];
        if (!slot.hasBox || !slot.itemData)
            return null;

        if (!boxPrefab)
        {
            Debug.LogError("[BoxInventory] boxPrefab not assigned");
            return null;
        }

        var go = Instantiate(
            boxPrefab,
            spawnPoint.position,
            spawnPoint.rotation);

        var core = go.GetComponent<BoxCore>();
        var itemInst = go.GetComponentInChildren<DeliveryItemInstance>();

        if (!core || !itemInst)
        {
            Debug.LogError("[BoxInventory] Box prefab missing BoxCore or DeliveryItemInstance");
            return null;
        }

        core.boxType = slot.boxType;
        itemInst.data = slot.itemData;
        itemInst.currentQuality = slot.itemQuality;
        core.SetAsCurrent();

        // clear slot
        slot.hasBox = false;
        slot.itemData = null;

        Debug.Log($"[BoxInventory] Spawned box from slot {slotIndex}");
        return core;
    }
}
