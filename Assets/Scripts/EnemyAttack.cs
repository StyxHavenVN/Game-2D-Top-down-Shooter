using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public int damage = 10;
    public float lifetime = 4f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        Health playerHealth = other.GetComponent<Health>();

        if (playerHealth == null)
        {
            playerHealth = other.GetComponentInParent<Health>();
        }

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}