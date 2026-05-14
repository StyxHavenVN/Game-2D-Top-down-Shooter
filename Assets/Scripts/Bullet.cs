using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Sát thương")]
    public int damage = 15; 

    [Header("Tự hủy")]
    public float lifetime = 3f;

    [Header("Hiệu ứng máu")]
    public GameObject bloodPrefabs;
    private PlayerStats playerStats;

    void Start()
    {
        playerStats = FindObjectOfType<PlayerStats>();

        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        EnemyBase enemy = other.GetComponent<EnemyBase>();

        if (enemy == null)
        {
            enemy = other.GetComponentInParent<EnemyBase>();
        }

        if (enemy == null) return;

        int finalDamage = damage; 

        if (playerStats != null)
        {
            // Tính toán sát thương mới: Gốc + (Gốc * Hệ số Buff)
            float finalDamageFloat = damage + (damage * playerStats.atkMultiplier);

            finalDamage = Mathf.RoundToInt(finalDamageFloat);
        }
        enemy.TakeDamage(finalDamage);

        if (bloodPrefabs != null)
        {
            GameObject blood = Instantiate(
                bloodPrefabs,
                transform.position,
                Quaternion.identity
            );

            Destroy(blood, 1f);
        }

        Destroy(gameObject);
    }
}