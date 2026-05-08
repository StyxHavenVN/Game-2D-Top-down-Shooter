using UnityEngine;
using UnityEngine.UI;
using System.Collections; // BẮT BUỘC THÊM: Thư viện này để dùng được hàm chờ thời gian (Coroutine)

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 50;
    private int currentHealth;

    [Header("Giao diện Máu")]
    public Image healthFill;

    [Header("Phần thưởng")]
    public int expReward = 10;

    [Header("Vật phẩm rơi ra")]
    public GameObject expOrbPrefab;

    // --- CODE MỚI THÊM CHO SHADER CHỚP TRẮNG ---
    [Header("Hiệu ứng dính đạn")]
    public SpriteRenderer spriteRenderer;
    public float flashDuration = 0.1f; // Thời gian nhá sáng (0.1 giây là vừa đẹp)

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthBar();

        // Tự động lấy Sprite Renderer (cái chứa Material) trên người con quái
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        Debug.Log(gameObject.name + " bị dính đòn! Máu còn: " + currentHealth);
        UpdateHealthBar();

        // THÊM MỚI: Gọi hiệu ứng chớp trắng ngay khi dính đạn
        if (spriteRenderer != null && spriteRenderer.material.HasProperty("_FlashAmount"))
        {
            StartCoroutine(FlashRoutine());
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void UpdateHealthBar()
    {
        if (healthFill != null)
        {
            healthFill.fillAmount = (float)currentHealth / maxHealth;
        }
    }

    void Die()
    {
        Debug.Log(gameObject.name + " đã bị tiêu diệt!");

        if (expOrbPrefab != null)
        {
            Instantiate(expOrbPrefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    // --- HÀM CHẠY NGẦM ĐỂ BẬT/TẮT CHỚP TRẮNG ---
    private IEnumerator FlashRoutine()
    {
        // 1. Kéo thanh Flash Amount lên 1 (Biến thành màu trắng)
        spriteRenderer.material.SetFloat("_FlashAmount", 1f);

        // 2. Chờ đúng 0.1 giây
        yield return new WaitForSeconds(flashDuration);

        // 3. Kéo thanh Flash Amount về 0 (Trở lại màu gốc)
        spriteRenderer.material.SetFloat("_FlashAmount", 0f);
    }
}