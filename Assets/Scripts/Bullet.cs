using UnityEngine;

public class BulletDamage : MonoBehaviour
{
    public int damage = 15; // Sát thương của viên đạn
    public float lifetime = 3f; // Tự hủy viên đạn sau 3 giây bay nếu không trúng ai để tránh nặng máy
    public GameObject bloodPrefabs;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Phải kiểm tra xem có trúng "Enemy" không đã
        if (other.CompareTag("Enemy"))
        {
            EnemyHealth enemy = other.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                GameObject blood = Instantiate(bloodPrefabs, transform.position, Quaternion.identity);
                Destroy(blood, 1f);
            }
            Destroy(gameObject);
        }
    }
}