using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 50;
    private int currentHealth;

    [Header("Giao diện Máu")]
    public Image healthFill;

    [Header("Phần thưởng")]
    public int expReward = 10;

    [Tooltip("Kéo Prefab ExpOrb vào đây")]
    public GameObject expOrbPrefab; // ← Đã thêm biến chứa Prefab Ngọc EXP

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthBar();
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log(gameObject.name + " bị dính đòn! Máu còn: " + currentHealth);
        UpdateHealthBar();

        if (currentHealth <= 0)
            Die();
    }

    void UpdateHealthBar()
    {
        if (healthFill != null)
            healthFill.fillAmount = (float)currentHealth / maxHealth;
    }

    void Die()
    {
        Debug.Log(gameObject.name + " đã bị tiêu diệt!");

        // Đã tắt phần cộng thẳng EXP vào người chơi
        /* PlayerStats playerStats = FindAnyObjectByType<PlayerStats>();
        if (playerStats != null)
            playerStats.AddExp(expReward);
        */

        // Rớt ngọc EXP ra đất tại vị trí quái chết
        if (expOrbPrefab != null)
        {
            Instantiate(expOrbPrefab, transform.position, Quaternion.identity);
        }

        // Báo kill lên GameManager
        if (GameManager.Instance != null)
            GameManager.Instance.RegisterKill();

        Destroy(gameObject);
    }
}