using UnityEngine;

public class SwordWeapon : WeaponBase
{
    [Header("Cài đặt riêng của Kiếm")]
    public GameObject swordSlashPrefab; // Prefab nhát chém (Vì nó không phải viên đạn bay đi nên không dùng Bullet Pool)

    public override void Attack(Vector2 direction, float angle)
    {
        if (weaponData == null || swordSlashPrefab == null) return;

        nextAttackTime = Time.time + (1f / weaponData.fireRate);

        // Vị trí xuất hiện nhát chém (cách nhân vật một khoảng range)
        Vector3 spawnPos = transform.position + (Vector3)(direction.normalized * weaponData.range);

        // Tạo nhát chém
        GameObject slash = Instantiate(swordSlashPrefab, spawnPos, Quaternion.Euler(0, 0, angle));

        MeleeEffect melee = slash.GetComponent<MeleeEffect>();
        if (melee != null)
        {
            melee.damage = Mathf.RoundToInt(weaponData.baseDamage);

            // Kiểm tra hiệu ứng Hút Máu
            if (inventory != null)
            {
                melee.lifestealPercent = inventory.lifestealPercent;
                melee.ownerHealth = GetComponentInParent<Health>(); 
            }
        }

        // Phát âm thanh chém
        if (audioManager != null) audioManager.PlaySwordSlash();
    }
}
