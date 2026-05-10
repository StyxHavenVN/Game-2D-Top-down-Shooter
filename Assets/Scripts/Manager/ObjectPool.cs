using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Hệ thống Object Pool giúp tái sử dụng đạn và quái vật thay vì dùng Instantiate/Destroy liên tục.
/// Việc này giúp tăng FPS đáng kể khi game có nhiều đạn/quái.
/// </summary>
public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance { get; private set; }

    [Header("Bullet Pool Settings")]
    public GameObject bulletPrefab;
    public int bulletPoolSize = 50;
    
    // Hàng đợi chứa các viên đạn đang KHÔNG được sử dụng (nằm trong kho)
    private Queue<GameObject> bulletPool = new Queue<GameObject>();

    private void Awake()
    {
        // Singleton pattern: Chỉ cho phép tồn tại 1 ObjectPool duy nhất
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Tạo sẵn một lượng đạn đưa vào kho (Pool)
        InitializePool();
    }

    private void InitializePool()
    {
        if (bulletPrefab == null) return;

        for (int i = 0; i < bulletPoolSize; i++)
        {
            GameObject obj = Instantiate(bulletPrefab);
            obj.SetActive(false); // Tắt đi, giấu vào kho
            obj.transform.SetParent(transform); // Nhét vào làm con của ObjectPool cho gọn Hierarchy
            bulletPool.Enqueue(obj);
        }
    }

    /// <summary>
    /// Gọi hàm này để LẤY một viên đạn ra bắn, thay cho Instantiate.
    /// Ví dụ: GameObject bullet = ObjectPool.Instance.GetBullet(vitri, gocXoay);
    /// </summary>
    public GameObject GetBullet(Vector3 position, Quaternion rotation)
    {
        GameObject bullet = null;

        if (bulletPool.Count > 0)
        {
            // Lấy 1 viên đạn từ kho ra
            bullet = bulletPool.Dequeue();
        }
        else
        {
            // Nếu kho lỡ hết đạn (bắn quá nhanh), đành phải tạo mới thêm 1 viên
            bullet = Instantiate(bulletPrefab);
            bullet.transform.SetParent(transform);
        }

        // Kích hoạt viên đạn và đặt vị trí
        bullet.SetActive(true);
        bullet.transform.position = position;
        bullet.transform.rotation = rotation;

        return bullet;
    }

    /// <summary>
    /// Gọi hàm này để TRẢ viên đạn về kho, thay cho Destroy.
    /// Ví dụ: ObjectPool.Instance.ReturnBullet(gameObject);
    /// </summary>
    public void ReturnBullet(GameObject bullet)
    {
        bullet.SetActive(false); // Tắt viên đạn đi
        
        // Reset lại vận tốc nếu đạn có Rigidbody2D
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        bulletPool.Enqueue(bullet); // Cất lại vào kho
    }
}
