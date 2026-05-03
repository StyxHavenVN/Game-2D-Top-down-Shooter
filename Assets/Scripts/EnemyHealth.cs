using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 50;
    private int currentHealth;

    [Header("Giao diện Máu")]
    public Image healthFill;

    // THÊM MỚI: Lượng Kinh nghiệm rớt ra khi con quái này chết
    [Header("Phần thưởng")]
    public int expReward = 10;

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

        // SỬA LẠI Ở ĐÂY: Tìm cục PlayerStats và cộng điểm EXP
        PlayerStats playerStats = FindAnyObjectByType<PlayerStats>();
        if (playerStats != null)
        {
            // Gọi hàm AddExp nằm trong file PlayerStats.cs
            playerStats.AddExp(expReward);
        }

        Destroy(gameObject);
    }
}