using UnityEngine;

public class ShotgunWeapon : WeaponBase
{
    public override void Attack(Vector2 direction, float angle)
    {
        if (weaponData == null || pool == null) return;

        // Tính thời gian cho viên đạn tiếp theo
        nextAttackTime = Time.time + (1f / weaponData.fireRate);

        int pellets = weaponData.pelletCount;
        float spread = weaponData.spreadAngle;
        
        float angleStep = pellets > 1 ? spread / (pellets - 1) : 0f;
        float startAngle = angle - (spread / 2f);

        for (int i = 0; i < pellets; i++)
        {
            float pelletAngle = startAngle + (angleStep * i);
            
            // Lấy đạn từ Pool
            GameObject pellet = pool.GetBullet(transform.position, Quaternion.Euler(0, 0, pelletAngle));

            Vector2 pelletDir = new Vector2(Mathf.Cos(pelletAngle * Mathf.Deg2Rad), Mathf.Sin(pelletAngle * Mathf.Deg2Rad));

            Rigidbody2D rb = pellet.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.AddForce(pelletDir.normalized * weaponData.bulletSpeed, ForceMode2D.Impulse);
            }

            BulletDamage bd = pellet.GetComponent<BulletDamage>();
            if (bd != null)
            {
                bd.damage = Mathf.RoundToInt(weaponData.baseDamage);

                // Kiểm tra hiệu ứng đặc biệt
                if (inventory != null)
                {
                    bd.isBurn = inventory.hasBurn;
                }
            }
        }

        // Phát âm thanh 1 lần cho cả chùm đạn
        if (audioManager != null) audioManager.PlayShotgunShoot();

        // Rung camera MẠNH khi bắn shotgun (cảm giác "đùng" rất đã)
        if (CameraShake.Instance != null)
            CameraShake.Instance.Shake(0.1f, 0.1f);
    }
}
