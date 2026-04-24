using UnityEngine;
using UnityEngine.UI; // Thêm dòng này để gọi UI
using TMPro; // Thêm dòng này để gọi chữ TextMeshPro

public class Health : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Giao Dien Thanh Mau")]
    public Image healthFillImage; // Kéo HealthBar_Fill vào đây
    public TextMeshProUGUI hpText; // Kéo HP_Text vào đây

    [Header("Quản lý Game Over")]
    public GameOverManager gameOverManager;
    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            if (gameOverManager != null)
            {
                gameOverManager.ShowGameOver(); // Kích hoạt Game Over!
            }
        }

        UpdateHealthUI();
    }

    // Hàm chuyên dùng để cập nhật giao diện
    void UpdateHealthUI()
    {
        if (healthFillImage != null)
        {
            // Tính phần trăm máu (từ 0.0 đến 1.0) để báo cho thanh Fill Amount
            healthFillImage.fillAmount = (float)currentHealth / maxHealth;
        }

        if (hpText != null)
        {
            hpText.text = currentHealth + " / " + maxHealth;
        }
    }
}