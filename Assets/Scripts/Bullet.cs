using UnityEngine;

public class BulletDamage : MonoBehaviour
{
    public int damage = 15; // Sát thương của viên đạn
    public float lifetime = 3f; // Tự hủy viên đạn sau 3 giây bay nếu không trúng ai để tránh nặng máy
    public GameObject bloodPrefabs;

    [Header("Hiệu ứng Đặc biệt")]
    public bool isPierce = false;
    public bool isBurn = false;

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

                // Nếu có hiệu ứng Burn, đốt cháy quái
                if (isBurn)
                {
                    enemy.ApplyBurn(damage * 0.05f, 5f); // Đốt 5% sát thương mỗi giây, kéo dài 5s
                }

                GameObject blood = Instantiate(bloodPrefabs, transform.position, Quaternion.identity);
                Destroy(blood, 1f);
            }
            
            // Nếu không có hiệu ứng Xuyên thấu (Pierce) thì hủy viên đạn
            if (!isPierce)
            {
                Destroy(gameObject);
            }
        }
    }
}