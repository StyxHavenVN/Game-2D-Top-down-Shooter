using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    [Header("Dữ liệu Quái (Kéo file EnemyData vào đây)")]
    public EnemyData enemyData;

    [HideInInspector]
    public float maxHealth = 50f;
    private float currentHealth;

    [Header("Giao diện Máu")]
    public Image healthFill;

    [Tooltip("Kéo Prefab ExpOrb vào đây")]
    public GameObject expOrbPrefab;

    [Header("Đẩy Lùi (Knockback)")]
    [Tooltip("Lực đẩy lùi khi trúng đạn")]
    public float knockbackForce = 5f;

    private Rigidbody2D rb;

    void Awake()
    {
        // Khởi tạo máu gốc từ thẻ Data (ScriptableObject)
        if (enemyData != null)
        {
            maxHealth = enemyData.maxHP;
        }
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthBar();
    }

    /// <summary>
    /// Reset máu về đầy (gọi khi lấy quái ra từ Object Pool).
    /// </summary>
    public void ResetHealth()
    {
        if (enemyData != null)
            maxHealth = enemyData.maxHP;

        currentHealth = maxHealth;
        isBurning = false;
        burnDuration = 0f;
        UpdateHealthBar();
    }

    void OnEnable()
    {
        // Mỗi lần quái được bật lên (lấy ra từ Pool), reset máu
        currentHealth = maxHealth;
        isBurning = false;
    }

    // Nhận sát thương (không có knockback — dùng cho Burn, AoE, v.v.)
    public void TakeDamage(float damage)
    {
        TakeDamage(damage, Vector2.zero);
    }

    /// <summary>
    /// Nhận sát thương VỚI lực đẩy lùi (Knockback).
    /// hitDirection = hướng viên đạn bay tới (từ đạn → quái).
    /// </summary>
    public void TakeDamage(float damage, Vector2 hitDirection)
    {
        currentHealth -= damage;
        UpdateHealthBar();

        // Hiển thị Popup Sát thương
        DamagePopup.Create(transform.position, (int)damage);

        // ⚡ FLASH TRẮNG khi trúng đạn
        EnemyFlash flash = GetComponent<EnemyFlash>();
        if (flash != null)
            flash.Flash();

        // 📸 Rung camera nhẹ khi quái trúng đạn
        if (CameraShake.Instance != null)
            CameraShake.Instance.Shake(0.05f, 0.05f);

        // 💥 ĐẨY LÙI QUÁI theo hướng viên đạn bay tới
        if (rb != null && hitDirection != Vector2.zero)
        {
            rb.AddForce(hitDirection.normalized * knockbackForce, ForceMode2D.Impulse);
        }

        // Phát âm thanh khi quái bị trúng đòn
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayEnemyHit();

        if (currentHealth <= 0)
            Die();
    }

    void UpdateHealthBar()
    {
        if (healthFill != null)
            healthFill.fillAmount = currentHealth / maxHealth;
    }

    void Die()
    {
        // Rớt ngọc EXP
        if (expOrbPrefab != null)
        {
            Instantiate(expOrbPrefab, transform.position, Quaternion.identity);
        }

        // Tự động rớt Vật phẩm Đặc Biệt (Xác suất % thông qua ItemDropManager)
        if (ItemDropManager.Instance != null)
        {
            ItemDropManager.Instance.TryDropItem(transform.position);
        }

        // Phát âm thanh quái chết
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayEnemyDeath();

        // Báo GameManager đếm Kill
        if (GameManager.Instance != null)
            GameManager.Instance.RegisterKill();

        // TRẢ QUÁI VỀ KHO (Object Pool) thay vì Destroy
        if (ObjectPool.Instance != null)
            ObjectPool.Instance.ReturnEnemy(gameObject);
        else
            gameObject.SetActive(false); // Fallback an toàn
    }

    // --- HIỆU ỨNG BURN ---
    private bool isBurning = false;
    private float burnDamagePerSec;
    private float burnDuration;

    public void ApplyBurn(float dps, float duration)
    {
        isBurning = true;
        burnDamagePerSec = dps;
        burnDuration = duration;
    }

    void Update()
    {
        // Xử lý logic đốt cháy theo thời gian
        if (isBurning)
        {
            burnDuration -= Time.deltaTime;
            currentHealth -= burnDamagePerSec * Time.deltaTime;
            UpdateHealthBar();

            // Hiển thị Popup sát thương Burn mỗi giây (dùng mẹo random để khỏi hiển thị liên tục mỗi frame)
            if (Random.Range(0, 100) < 5)
            {
                DamagePopup.Create(transform.position, Mathf.CeilToInt(burnDamagePerSec));
            }

            if (currentHealth <= 0) Die();
            if (burnDuration <= 0) isBurning = false;
        }
    }
}