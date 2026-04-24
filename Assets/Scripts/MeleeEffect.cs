using UnityEngine;

public class MeleeEffect : MonoBehaviour
{
    public int damage = 20; // Sát thương của nhát chém
    public float lifetime = 0.15f; // Thời gian tồn tại cực ngắn (ví dụ: một cái chớp mắt)

    void Start()
    {
        // Tự động xóa GameObject này đi sau 'lifetime' giây
        Destroy(gameObject, lifetime);
    }

    // Hàm này được gọi khi một vật thể khác đi vào vùng va chạm Trigger của nhát chém
    void OnTriggerEnter2D(Collider2D other)
    {
        // 1. Kiểm tra xem vật thể đó có phải là Quái vật không
        // (Chúng ta dùng Tag "Enemy" để phân biệt quái vật)
        if (other.CompareTag("Enemy"))
        {
            // 2. Nếu trúng quái vật, tìm và gọi hàm nhận sát thương của nó
            // (Giả sử bạn có script EnemyHealth nằm trên quái vật)
            EnemyHealth enemy = other.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }

            // Thêm dòng này để test xem đã chém trúng chưa
            Debug.Log("Đã chém trúng quái vật: " + other.name);

            // Mẹo: Bạn có thể thêm hiệu ứng máu chảy (Blood VFX) hoặc âm thanh chém trúng tại đây
        }
    }
}