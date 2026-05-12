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

    void Awake()
    {
        // Khởi tạo máu gốc từ thẻ Data (ScriptableObject)
        if (enemyData != null)
        {
            maxHealth = enemyData.maxHP;
        }
    }

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthBar();
    }

    // Nhận sát thương
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        UpdateHealthBar();

        // Hiển thị Popup Sát thương
        DamagePopup.Create(transform.position, (int)damage);

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

        // Báo GameManager đếm Kill
        if (GameManager.Instance != null)
            GameManager.Instance.RegisterKill();

        Destroy(gameObject);
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
            // Gọi trừ máu trực tiếp không dùng UpdateHealthBar liên tục để tránh spam hiệu ứng nếu có
            currentHealth -= burnDamagePerSec * Time.deltaTime;
            UpdateHealthBar();

            // Hiển thị Popup sát thương Burn mỗi giây (dùng mẹo random để khỏi hiển thị liên tục mỗi frame)
            if (Random.Range(0, 100) < 5) // Tỉ lệ hiện popup rất nhỏ mỗi frame
            {
                DamagePopup.Create(transform.position, Mathf.CeilToInt(burnDamagePerSec));
            }

            if (currentHealth <= 0) Die();
            if (burnDuration <= 0) isBurning = false;
        }
    }
}