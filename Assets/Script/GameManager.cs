using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Time Settings")]
    public int currentDay = 1;
    public int currentHour = 8;   // เริ่ม 08.00
    public int currentMinute = 0;

    [Tooltip("เวลาจริง (วินาที) ต่อ 1 ชั่วโมงในเกม")]
    public float realSecondsPerGameHour = 30f;

    float timeAcc;   // ตัวนับเวลาจริง

    [Header("Money")]
    public int totalMoney = 0;

    [Header("Delivery Storage (max 3 per round)")]
    public int maxActiveBoxes = 3;

    [System.Serializable]
    public class DeliveryRecord
    {
        public BoxCore box;
        public DeliveryItemInstance itemInstance;
        public DeliveryItemData data;
        public int dayCreated;
    }

    // กล่องที่อยู่ใน "รอบนี้" (สูงสุด 3)
    public List<DeliveryRecord> activeBoxes = new();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        UpdateGameTime();
    }

    void UpdateGameTime()
    {
        timeAcc += Time.deltaTime;

        float secPerHour = realSecondsPerGameHour;
        while (timeAcc >= secPerHour)
        {
            timeAcc -= secPerHour;
            AdvanceOneHour();
        }
    }

    void AdvanceOneHour()
    {
        currentHour++;
        if (currentHour >= 24)
        {
            currentHour = 0;
            currentDay++;
        }
        currentMinute = 0;
    }


    // ========= API เกี่ยวกับเงิน =========

    public void AddMoney(int amount)
    {
        totalMoney += amount;
        if (totalMoney < 0) totalMoney = 0;
        Debug.Log($"[GameManager] Money: {totalMoney}");
    }

    // ========= API เกี่ยวกับกล่องในรอบนี้ =========

    /// <summary>
    /// เรียกจาก BoxCore เมื่อกล่องพร้อมส่ง (ปิดเทป+ลาเบลแล้ว)
    /// </summary>
    public void RegisterNewDelivery(BoxCore box, DeliveryItemInstance item)
    {
        if (!box || !item || !item.data) return;

        // ถ้าเต็ม 3 กล่องแล้ว ให้คุณตัดสินใจเองว่าจะทำอย่างไร
        if (activeBoxes.Count >= maxActiveBoxes)
        {
            Debug.LogWarning("[GameManager] Active boxes is full (3). ต้องไปจัดการ UI เอาออกก่อน");
            return;
        }

        var record = new DeliveryRecord
        {
            box = box,
            itemInstance = item,
            data = item.data,
            dayCreated = currentDay
        };
        activeBoxes.Add(record);

        Debug.Log($"[GameManager] Register box: {record.data.itemName} day={currentDay}");
    }

    /// <summary>
    /// เรียกตอนส่งถึงปลายทาง
    /// </summary>
    public void CompleteDelivery(BoxCore box)
    {
        DeliveryRecord rec = null;
        foreach (var r in activeBoxes)
        {
            if (r.box == box)
            {
                rec = r;
                break;
            }
        }
        if (rec == null) return;

        // คำนวณเงินจากตัว item
        int reward = rec.itemInstance.CalculateReward(rec.dayCreated, currentDay);
        AddMoney(reward);

        Debug.Log($"[GameManager] Delivery complete {rec.data.itemName}, reward={reward}");

        activeBoxes.Remove(rec);
    }
}
