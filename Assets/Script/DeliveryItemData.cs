using UnityEngine;

public enum ItemCategory
{
    Normal,
    Cold,       // ต้องเก็บความเย็น
    Fragile,    // แตกง่าย
    Liquid,
}

public enum BoxKind
{
    Small,
    Medium,
    Large,
    ColdBox
}

[CreateAssetMenu(
    fileName = "DeliveryItemData",
    menuName = "SendGame/Delivery Item Data")]
public class DeliveryItemData : ScriptableObject
{

    public string itemName;
    public Sprite icon;
    public ItemCategory category = ItemCategory.Normal;
    public bool breaksOnWater = true;
    public int deliveryLimitDays = 3;
    public int baseReward = 100;

    [Header("Quality Range(0, 100)")]
    public int baseQuality = 100;

    [Header("Destination")]
    public string destinationId;

    [Header("Damage Settings")]
    public float safeImpactVelocity = 3f;
    public float damagePerVelocity = 5f;

    [Header("Allowed Box Types")]
    public BoxKind[] allowedBoxTypes;
}
