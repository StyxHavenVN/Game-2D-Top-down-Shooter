using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "SilverBullet/WeaponData")]
public class WeaponData : ScriptableObject
{
    [Header("Identity")]
    public string weaponName;
    public Sprite weaponIcon;
    public WeaponType weaponType; // enum: Pistol, Shotgun, Sword

    [Header("Stats")]
    public float baseDamage;
    public float fireRate;        // shots per second
    public float bulletSpeed;     // for ranged weapons
    public float range;           // for sword hitbox radius

    [Header("Shotgun Only")]
    public int pelletCount = 1;   // 5 for shotgun
    public float spreadAngle = 0f;

    [Header("Special Effect")]
    public ItemEffectType specialEffect; // Pierce / Burn / Lifesteal
    
    [TextArea]
    public string description;
}

public enum WeaponType { Pistol, Shotgun, Sword }

public enum ItemEffectType
{
    Heal,
    Shield,
    Burn,
    Pierce,
    Lifesteal,
    None
}
