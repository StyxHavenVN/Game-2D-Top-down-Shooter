using UnityEngine;

public class PistolWeapon : WeaponBase
{
    public override void Attack(Vector2 direction, float angle)
    {
        if (weaponData == null || pool == null) return;

        // Tính thời gian cho viên đạn tiếp theo
        nextAttackTime = Time.time + (1f / weaponData.fireRate);

        // Lấy đạn từ Pool
        GameObject bullet = pool.GetBullet(transform.position, Quaternion.Euler(0, 0, angle));
        
        // Tạo lực đẩy đạn
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.AddForce(direction.normalized * weaponData.bulletSpeed, ForceMode2D.Impulse);
        }

        // Truyền sát thương
        BulletDamage bd = bullet.GetComponent<BulletDamage>();
        if (bd != null)
        {
            bd.damage = Mathf.RoundToInt(weaponData.baseDamage);

            // Kiểm tra hiệu ứng đặc biệt từ túi đồ
            if (inventory != null)
            {
                bd.isPierce = inventory.hasPierce;
            }
        }

        // Phát âm thanh
        if (audioManager != null) audioManager.PlayPistolShoot();

        // Rung camera nhẹ khi bắn súng lục
        if (CameraShake.Instance != null)
            CameraShake.Instance.Shake(0.02f, 0.02f);
    }
}
