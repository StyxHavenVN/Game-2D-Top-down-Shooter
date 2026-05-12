using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Quản lý máu của người chơi.
/// Hỗ trợ trạng thái miễn nhiễm sát thương (i-frames) khi đang Dash.
/// </summary>
public class Health : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Quản lý Game Over")]
    public GameOverManager gameOverManager;

    // Trạng thái miễn nhiễm — được PlayerDash bật/tắt
    private bool isInvincible = false;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    /// <summary>
    /// Nhận sát thương. Nếu đang i-frames (dash) thì bỏ qua hoàn toàn.
    /// </summary>
    public void TakeDamage(int damage)
    {
        // ← ĐIỂM QUAN TRỌNG: Bỏ qua mọi sát thương khi đang Dash
        if (isInvincible)
        {
            Debug.Log("[Health] Đang i-frames — miễn nhiễm sát thương!");
            return;
        }

        currentHealth -= damage;

        // 🔴 CHỚP ĐỎ MÀN HÌNH khi bị đánh (tạo cảm giác nguy hiểm!)
        if (DamageVignette.Instance != null)
            DamageVignette.Instance.Flash();

        // 📸 RUNG CAMERA MẠNH khi Player bị đánh (đau hơn quái trúng đạn)
        if (CameraShake.Instance != null)
            CameraShake.Instance.Shake(0.2f, 0.15f);

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            if (gameOverManager != null)
                gameOverManager.ShowGameOver();
        }

        UpdateHealthUI();
    }

    /// <summary>Bật hoặc tắt trạng thái miễn nhiễm sát thương (i-frames).</summary>
    public void SetInvincible(bool value)
    {
        isInvincible = value;
        Debug.Log($"[Health] I-frames: {(value ? "BẬT" : "TẮT")}");
    }

    /// <summary>Hồi máu cho người chơi (dùng cho item heal).</summary>
    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        UpdateHealthUI();
    }

    /// <summary>Trả về true nếu đang trong trạng thái miễn nhiễm.</summary>
    public bool IsInvincible() => isInvincible;

    // Cập nhật giao diện thanh máu qua UIManager
    private void UpdateHealthUI()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateHP(currentHealth, maxHealth);
        }
    }
}