using UnityEngine;

public class MiniEnemy : MeleeEnemy
{
    [Header("Mini Enemy")]
    public float lifeTime = 15f; // Sau 15 giây tự biến mất

    protected override void Start()
    {
        base.Start();

        enemyName = "Mini Enemy";

        // Nếu muốn ép chỉ số riêng cho mini enemy
        maxHealth = 25;
        currentHealth = maxHealth;

        moveSpeed = 4f;
        contactDamage = 3;
        damageCooldown = 0.7f;

        UpdateHealthBar();

        Destroy(gameObject, lifeTime);
    }

    protected override void Die()
    {
        if (isDead) return;

        isDead = true;

        // Mini enemy có thể KHÔNG tính kill, tránh farm kill từ boss summon
        // Nếu muốn tính kill thì mở đoạn này:
        /*
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterKill();
        }
        */

        // Mini enemy có thể KHÔNG rơi EXP, tránh farm EXP
        // Nếu muốn rơi EXP thì mở đoạn này:
        /*
        DropExp();
        */

        Destroy(gameObject);
    }
}