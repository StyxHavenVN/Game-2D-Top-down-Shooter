using UnityEngine;
using UnityEngine.UI; // CẦN THÊM DÒNG NÀY ĐỂ ĐIỀU KHIỂN UI

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 50;
    private int currentHealth;

    [Header("Giao diện Máu")]
    public Image healthFill; // Nơi gắn thanh máu đỏ vào

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthBar(); // Cập nhật thanh máu lúc mới sinh ra
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        Debug.Log(gameObject.name + " bị dính đòn! Máu còn: " + currentHealth);
        UpdateHealthBar(); // Gọi hàm cập nhật thanh máu mỗi khi mất máu

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // Hàm cập nhật giao diện
    void UpdateHealthBar()
    {
        if (healthFill != null)
        {
            // Tính toán tỷ lệ phần trăm (0.0 đến 1.0)
            healthFill.fillAmount = (float)currentHealth / maxHealth;
        }
    }

    void Die()
    {
        Debug.Log(gameObject.name + " đã bị tiêu diệt!");

        // MỚI THÊM: Cho Player 35 Kinh nghiệm khi con quái này chết
        if (PlayerExperience.instance != null)
        {
            PlayerExperience.instance.AddXP(35); // Bạn có thể đổi số 35 thành lượng XP tùy thích!
        }

        Destroy(gameObject);
    }
}