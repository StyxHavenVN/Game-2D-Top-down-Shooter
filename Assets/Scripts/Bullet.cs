using UnityEngine;

public class BulletDamage : MonoBehaviour
{
    public int damage = 15; // Sát thương của viên đạn
    public float lifetime = 3f; // Tự trả về kho sau 3 giây bay nếu không trúng ai
    public GameObject bloodPrefabs;

    [Header("Hiệu ứng Đặc biệt")]
    public bool isPierce = false;
    public bool isBurn = false;

    // Bộ đếm thời gian nội bộ (thay cho Destroy)
    private float timer;

    /// <summary>
    /// Reset trạng thái viên đạn khi lấy ra từ Pool.
    /// Được ObjectPool gọi tự động.
    /// </summary>
    public void ResetBullet()
    {
        timer = lifetime;
        isPierce = false;
        isBurn = false;
    }

    void OnEnable()
    {
        // Mỗi lần viên đạn được bật lên (lấy ra từ Pool), reset timer
        timer = lifetime;
    }

    void Update()
    {
        // Đếm ngược thời gian sống
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            ReturnToPool();
        }
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

                // Hiệu ứng máu
                if (bloodPrefabs != null)
                {
                    GameObject blood = Instantiate(bloodPrefabs, transform.position, Quaternion.identity);
                    Destroy(blood, 1f);
                }
            }

            // Nếu không có hiệu ứng Xuyên thấu (Pierce) thì trả đạn về kho
            if (!isPierce)
            {
                ReturnToPool();
            }
        }
    }

    /// <summary>
    /// Trả viên đạn về kho thay vì Destroy
    /// </summary>
    private void ReturnToPool()
    {
        if (ObjectPool.Instance != null)
        {
            ObjectPool.Instance.ReturnBullet(gameObject);
        }
        else
        {
            // Fallback: nếu ObjectPool không tồn tại thì Destroy bình thường
            gameObject.SetActive(false);
        }
    }
}