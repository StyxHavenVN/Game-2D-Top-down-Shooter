using UnityEngine;

public class Explosion : MonoBehaviour
{
    [Header("Sát thương vụ nổ")]
    public int damage = 30;

    [Header("Tự hủy")]
    public float lifeTime = 0.5f;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Gây damage cho Player
        if (other.CompareTag("Player"))
        {
            Health playerHealth = other.GetComponent<Health>();

            if (playerHealth == null)
            {
                playerHealth = other.GetComponentInParent<Health>();
            }

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
        }

    // Gây damage cho Enemy khác
    EnemyBase enemy = other.GetComponent<EnemyBase>();

        if (enemy == null)
        {
            enemy = other.GetComponentInParent<EnemyBase>();
        }

        if (enemy != null)
        {
            enemy.TakeDamage(damage);
        }
    }

    public void DestroyExplosion()
    {
        Destroy(gameObject);
    }
}