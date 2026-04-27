using UnityEngine;

public class BulletDamage : MonoBehaviour
{
    public int damage = 15; // Sát thương của viên đạn
    public float lifetime = 3f; // Tự hủy viên đạn sau 3 giây bay nếu không trúng ai để tránh nặng máy

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    // Hàm này chạy khi viên đạn đâm vào một Collider khác (được đánh dấu là Is Trigger)
    void OnTriggerEnter2D(Collider2D other)
    {
        // Phải kiểm tra xem có trúng "Enemy" không đã
        if (other.CompareTag("Enemy"))
        {
            EnemyHealth enemy = other.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }

            // Lệnh HỦY ĐẠN BẮT BUỘC PHẢI NẰM BÊN TRONG CÁI NGOẶC NHỌN CỦA IF NÀY
            Destroy(gameObject);
        }
    }
}