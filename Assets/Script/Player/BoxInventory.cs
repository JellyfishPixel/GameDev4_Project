using System;
using UnityEngine;

public class BoxInventory : MonoBehaviour
{
    public static BoxInventory Instance { get; private set; }

    [Header("Inventory Settings")]
    public int maxSlots = 3;

    [Header("Box Prefab")]
    public GameObject boxPrefab;

    [Header("Carry Protection")]
    [Tooltip("ตัวหารดาเมจตอนอยู่ใน inventory (2 = ครึ่งหนึ่ง, 3 = เหลือ 1/3)")]
    public int inventoryDamageDivisor = 2;

    [Serializable]
    public class BoxSlot
    {
        public bool hasBox;
        public BoxKind boxType;
        public DeliveryItemData itemData;

        [Header("QUALITY")]
        [Range(0, 100)]
        public float itemQuality = 100f;

        [Header("DELIVERY TIME")]
        public int remainingDays = 0;

        [Header("PROTECTION SNAPSHOT")]
        [Tooltip("ตัวหารดาเมจรวมที่เซฟมาจากกล่อง + บับเบิล ตอนเก็บเข้าช่องนี้")]
        public int protectionDivisor = 1;

        [Tooltip("เปอร์เซ็นต์การเซฟดาเมจ (0–100%)")]
        public float protectionPercent = 0f;

        [Header("STATE")]
        public bool isDamaged;
        public bool isBroken;
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
            if (slots[i] == null)
                slots[i] = new BoxSlot();
    }

    public BoxSlot GetSlot(int idx)
    {
        if (idx < 0 || idx >= slots.Length) return null;
        return slots[idx];
    }

    int FindFirstFreeSlot()
    {
        for (int i = 0; i < slots.Length; i++)
            if (!slots[i].hasBox)
                return i;
        return -1;
    }

    void UpdateItemState(BoxSlot slot)
    {
        if (slot.itemData == null) return;

        slot.isDamaged = slot.itemQuality <= slot.itemData.damagedThreshold;
        slot.isBroken = slot.itemQuality <= slot.itemData.brokenThreshold;
    }
    public void AdvanceOneDay()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            var s = slots[i];
            if (!s.hasBox) continue;

            s.remainingDays--;

            if (s.remainingDays < 0)
                s.remainingDays = 0;

            Debug.Log($"[BoxInventory] Slot {i} remainingDays = {s.remainingDays}");
        }
    }
    // ================== DELIVERY FROM INVENTORY ==================

    public int FindSlotByDestination(string destId)
    {
        if (string.IsNullOrEmpty(destId) || slots == null) return -1;

        for (int i = 0; i < slots.Length; i++)
        {
            var s = slots[i];
            if (!s.hasBox || s.itemData == null) continue;

            // สมมติว่าใน DeliveryItemData มี field ชื่อ destinationId
            if (s.itemData.destinationId == destId)
                return i;
        }

        return -1;
    }

    public bool TryDeliverFromInventory(string destId, out int reward)
    {
        reward = 0;

        int slotIndex = FindSlotByDestination(destId);
        if (slotIndex < 0) return false;

        var slot = slots[slotIndex];
        if (!slot.hasBox || slot.itemData == null) return false;

        var data = slot.itemData;

        // ========= คำนวณค่า reward แบบง่าย =========
        float r = data.baseReward;                     // เงินพื้นฐานจาก Data
        float qualityFactor = Mathf.Clamp01(slot.itemQuality / 100f);
        r *= qualityFactor;                           // คุณภาพต่ำ → เงินน้อยลง

        // ถ้าคุณอยากให้ "ของพัง = 0 บาท" ง่าย ๆ:
        if (slot.itemQuality <= 0f)
            r = 0f;

        reward = Mathf.Max(0, Mathf.RoundToInt(r));

        // ลบของจาก inventory
        slot.hasBox = false;
        slot.itemData = null;
        // (slot.itemQuality ยังเก็บค่าล่าสุดไว้ได้ เผื่อ debug)

        Debug.Log($"[BoxInventory] DeliverFromInventory dest={destId}, reward={reward}");

        return true;
    }

    public bool StoreBox(BoxCore box)
    {
        if (!box || !box.CurrentItemData || !box.CurrentItemInstance)
        {
            Debug.LogWarning("[BoxInventory] StoreBox: Box หรือ ItemData/Instance ว่าง");
            return false;
        }

        int free = FindFirstFreeSlot();
        if (free < 0)
        {
            Debug.Log("[BoxInventory] StoreBox: Inventory เต็มแล้ว");
            return false;
        }
        var slot = slots[free];
        slot.hasBox = true;
        slot.boxType = box.boxType;
        slot.itemData = box.CurrentItemData;
        slot.itemQuality = box.CurrentItemInstance.currentQuality;
        slot.remainingDays =
            box.CurrentItemInstance.CalculateEffectiveDeadlineDays(
                box.CurrentItemData.deliveryLimitDays,
                box.boxType == BoxKind.ColdBox
            );

        // 🔹 ดึงค่าการเซฟจากกล่อง (รวมกล่อง + บับเบิล)
        int div = box.GetTotalDamageDivisor();
        slot.protectionDivisor = div;

        float p01 = box.GetProtection01();
        slot.protectionPercent = p01 * 100f;

        // เซ็ตสถานะ
        UpdateItemState(slot);

        Debug.Log($"[BoxInventory] StoreBox: slot={free}, item={slot.itemData.itemName}, " +
                  $"Q={slot.itemQuality:F1}, protectDiv={slot.protectionDivisor}, save={slot.protectionPercent:F0}%");

        Destroy(box.gameObject);
        return true;
    }

    // ---------------- เอากล่องจาก inventory ออกมาในโลก ----------------
    public BoxCore SpawnBoxFromSlot(int slotIndex, Transform spawnPoint)
    {
        if (slotIndex < 0 || slotIndex >= slots.Length)
        {
            Debug.LogWarning($"[BoxInventory] SpawnBoxFromSlot: index {slotIndex} ไม่ถูกต้อง");
            return null;
        }

        var slot = slots[slotIndex];
        if (!slot.hasBox || slot.itemData == null)
        {
            Debug.Log($"[BoxInventory] SpawnBoxFromSlot: slot {slotIndex} ว่าง");
            return null;
        }

        if (!boxPrefab)
        {
            Debug.LogError("[BoxInventory] boxPrefab ยังไม่ได้เซ็ต");
            return null;
        }

        GameObject go = Instantiate(boxPrefab, spawnPoint.position, spawnPoint.rotation);
        var core = go.GetComponent<BoxCore>();
        var itemInst = go.GetComponentInChildren<DeliveryItemInstance>();

        if (!core || !itemInst)
        {
            Debug.LogError("[BoxInventory] prefab ไม่มี BoxCore หรือ DeliveryItemInstance");
            return null;
        }

        core.boxType = slot.boxType;
        itemInst.data = slot.itemData;
        itemInst.currentQuality = slot.itemQuality;
        core.SetAsCurrent();

        Debug.Log($"[BoxInventory] SpawnBoxFromSlot: เอา {slot.itemData.itemName} ออกจาก slot {slotIndex} ด้วย quality={slot.itemQuality:F1}");

        slot.hasBox = false;
        slot.itemData = null;
        // slot.itemQuality จะยังเก็บค่าล่าสุดไว้ (ใช้ debug ได้)

        return core;
    }

    public void ApplyFallDamageToAll(float fallHeight)
    {
        int meters = Mathf.RoundToInt(fallHeight);
        if (meters <= 0) return;

        for (int i = 0; i < slots.Length; i++)
        {
            var s = slots[i];
            if (!s.hasBox || s.itemData == null) continue;

            var data = s.itemData;
            if (meters < data.minFallHeightMeter) continue;

            int perMeter = Mathf.Max(0, data.damagePerMeter);
            int raw = perMeter * meters;

            // 🔹 ใช้ตัวหารเดียวกับที่กล่องเซฟมา (กล่อง + บับเบิล)
            int divisor = Mathf.Max(1, s.protectionDivisor);
            int dmg = raw / divisor;
            if (dmg <= 0) dmg = 1;

            float oldQ = s.itemQuality;
            s.itemQuality = Mathf.Clamp(oldQ - dmg, 0f, 100f);
            UpdateItemState(s);

            Debug.Log($"[BoxInventory] slot {i} {data.itemName}: fall={fallHeight:F2}m " +
                      $"({meters}m), raw={raw}, div={divisor}, dmg={dmg}, Q {oldQ:F0}→{s.itemQuality:F0}");
        }
    }


    public int GetUsedSlotCount()
    {
        int count = 0;
        if (slots == null) return 0;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null && slots[i].hasBox)
                count++;
        }

        return count;
    }


}
