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
}