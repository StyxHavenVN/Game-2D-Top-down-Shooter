using UnityEngine;
using System.Collections.Generic;

public class MeleeEffect : MonoBehaviour
{
    [Header("Sát thương của kiếm")]
    public int damage = 20;

    [Header("Thời gian tồn tại của nhát chém")]
    public float lifetime = 0.15f;

    [Header("Hút máu")]
    public int lifeStealAmount = 5;

    private Health playerHealth;

    // Tránh 1 nhát chém dính nhiều collider của cùng 1 quái rồi hút máu nhiều lần
    private HashSet<EnemyBase> damagedEnemies = new HashSet<EnemyBase>();

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            playerHealth = player.GetComponent<Health>();

            if (playerHealth == null)
            {
                playerHealth = player.GetComponentInParent<Health>();
            }
        }

        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;

        EnemyBase enemy = other.GetComponent<EnemyBase>();

        if (enemy == null)
        {
            enemy = other.GetComponentInParent<EnemyBase>();
        }

        if (enemy == null) return;

        if (damagedEnemies.Contains(enemy)) return;

        damagedEnemies.Add(enemy);

        enemy.TakeDamage(damage);

        if (playerHealth != null && lifeStealAmount > 0)
        {
            playerHealth.Heal(lifeStealAmount);
            Debug.Log("Kiếm hút máu: +" + lifeStealAmount + " HP");
        }

        Debug.Log("Đã chém trúng quái vật: " + other.name);
    }
}