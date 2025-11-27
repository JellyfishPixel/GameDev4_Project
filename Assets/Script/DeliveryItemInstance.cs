using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class DeliveryItemInstance : MonoBehaviour
{
    [Header("Data")]
    public DeliveryItemData data;   // ลาก ScriptableObject มาใส่

    [Header("Runtime State (read-only)")]
    [Range(0, 100)] public float currentQuality = 100f;
    public bool isDamaged;
    public bool isBroken;

    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (data != null)
            currentQuality = data.baseQuality;
    }

    // ---------- ความเสียหายจากการตกแรง ----------
    void OnCollisionEnter(Collision collision)
    {
        if (data == null || isBroken) return;

        float impact = collision.relativeVelocity.magnitude;

        if (impact > data.safeImpactVelocity)
        {
            float over = impact - data.safeImpactVelocity;
            float damage = over * data.damagePerVelocity;
            ApplyDamage(damage);

            // debug ดูค่าชน
            Debug.Log($"[Item] Impact={impact:F2} damage={damage:F1}");
        }

        // ตัวอย่าง: โดนน้ำ
        if (collision.collider.CompareTag("Water") && data.breaksOnWater)
        {
            ApplyDamage(999f); // พังยับ
            Debug.Log("[Item] Hit water → broken.");
        }
    }

    void ApplyDamage(float amount)
    {
        currentQuality -= amount;
        currentQuality = Mathf.Clamp(currentQuality, 0f, 100f);

        if (amount > 0f) isDamaged = true;
        if (currentQuality <= 0f) isBroken = true;

        // TODO: ตรงนี้เรียกอีเวนต์อัปเดต UI ได้ภายหลัง
    }


    public int CalculateReward(int dayCreated, int dayDelivered)
    {
        if (data == null) return 0;

        int daysUsed = Mathf.Max(0, dayDelivered - dayCreated);

        // 1) base reward
        float reward = data.baseReward;

        // 2) หักตามคุณภาพ
        float qualityFactor = currentQuality / 100f;
        reward *= qualityFactor;

        // 3) หักถ้าส่งช้ากว่า limit
        if (daysUsed > data.deliveryLimitDays)
        {
            // ตัวอย่าง: ได้แค่ 50% ถ้าช้า
            reward *= 0.5f;
        }

        // 4) ถ้าของแตก (คุณภาพ 0) ได้ 0 เลย
        if (isBroken)
            reward = 0f;

        return Mathf.Max(0, Mathf.RoundToInt(reward));
    }
}
