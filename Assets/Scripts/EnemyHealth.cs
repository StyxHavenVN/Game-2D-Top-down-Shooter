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
    private bool isDead = false; // Ngăn Die() gọi nhiều lần

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
        isDead = false;
        isBurning = false;
        burnDuration = 0f;
        UpdateHealthBar();
    }

    void OnEnable()
    {
        // Mỗi lần quái được bật lên (lấy ra từ Pool), reset máu
        currentHealth = maxHealth;
        isDead = false;
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
        if (isDead) return; // Không nhận thêm sát thương nếu đã chết

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
        if (isDead) return; // Ngăn gọi Die() nhiều lần (ví dụ burn + đạn cùng lúc)
        isDead = true;

        // Dừng burn ngay lập tức
        isBurning = false;

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
        {
            GameManager.Instance.RegisterKill();

            // Nếu là Boss → kích hoạt chiến thắng!
            if (enemyData != null && enemyData.isBoss)
            {
                GameManager.Instance.BossDefeated();
            }
        }

        // Boss dùng Destroy (không nằm trong Pool), quái thường trả về Pool
        if (enemyData != null && enemyData.isBoss)
        {
            Destroy(gameObject, 0.1f); // Delay nhẹ để BossEnemy.OnDisable() chạy trước
        }
        else if (ObjectPool.Instance != null)
        {
            // TRẢ QUÁI VỀ KHO (Object Pool) thay vì Destroy
            ObjectPool.Instance.ReturnEnemy(gameObject);
        }
        else
        {
            gameObject.SetActive(false); // Fallback an toàn
        }
    }

    // --- HIỆU ỨNG BURN ---
    private bool isBurning = false;
    private float burnDamagePerSec;
    private float burnDuration;
    private float burnTickTimer = 0f; // Bộ đếm hiển thị sát thương burn

    public void ApplyBurn(float dps, float duration)
    {
        isBurning = true;
        burnDamagePerSec = dps;
        burnDuration = duration;
        burnTickTimer = 0f;
    }

    void Update()
    {
        if (isDead) return;

        // Xử lý logic đốt cháy theo thời gian
        if (isBurning)
        {
            burnDuration -= Time.deltaTime;
            float burnDmgThisFrame = burnDamagePerSec * Time.deltaTime;
            currentHealth -= burnDmgThisFrame;
            UpdateHealthBar();

            // Hiển thị Popup sát thương Burn mỗi 0.5 giây (thay vì random mỗi frame)
            burnTickTimer += Time.deltaTime;
            if (burnTickTimer >= 0.5f)
            {
                DamagePopup.Create(transform.position, Mathf.CeilToInt(burnDamagePerSec * 0.5f));
                burnTickTimer = 0f;
            }

            if (currentHealth <= 0) Die();
            if (burnDuration <= 0) isBurning = false;
        }
    }
}