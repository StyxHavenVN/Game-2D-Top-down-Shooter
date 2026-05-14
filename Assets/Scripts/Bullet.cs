using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Sát thương")]
    public int damage = 15;

    [Header("Tự hủy")]
    public float lifetime = 3f;

    [Header("Hiệu ứng máu")]
    public GameObject bloodPrefabs;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Tìm EnemyBase trên object bị trúng hoặc object cha
        EnemyBase enemy = other.GetComponent<EnemyBase>();

        if (enemy == null)
        {
            enemy = other.GetComponentInParent<EnemyBase>();
        }

        // Nếu không phải enemy thì bỏ qua
        if (enemy == null) return;

        enemy.TakeDamage(damage);

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