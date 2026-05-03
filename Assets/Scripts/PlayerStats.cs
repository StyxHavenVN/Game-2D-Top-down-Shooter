using UnityEngine;
using UnityEngine.UI; // Thêm dòng này để xài Image (Thanh máu/exp)
using TMPro;          // Thêm dòng này để xài TextMeshPro

public class PlayerStats : MonoBehaviour
{
    [Header("Thông tin Cấp độ")]
    public int level = 1;
    public int currentExp = 0;
    public int expToNextLevel = 10;

    [Header("Hệ số nâng cấp (Buff)")]
    public float atkMultiplier = 0f;
    public float defMultiplier = 0f;

    [Header("Giao diện UI")]
    public TextMeshProUGUI levelText;  // Kéo chữ Level vào đây
    public Image expBarFill;           // Kéo thanh ngang màu xanh lá vào đây
    public TextMeshProUGUI expText;    // Kéo chữ hiển thị 0/10 XP vào đây

    [Header("Tham chiếu Script khác")]
    public LevelUpManager levelUpManager;
    private Health playerHealth;

    void Start()
    {
        playerHealth = GetComponent<Health>();
        CalculateNextLevelExp();

        // QUAN TRỌNG: Cập nhật giao diện về Level 1 ngay khi vừa vào game
        UpdateExpUI();
    }

    public void AddExp(int expAmount)
    {
        currentExp += expAmount;
        Debug.Log($"Nhận được {expAmount} EXP! Hiện tại: {currentExp}/{expToNextLevel}");

        if (currentExp >= expToNextLevel)
        {
            LevelUp();
        }

        // Cập nhật lại UI sau khi nhận thêm điểm
        UpdateExpUI();
    }

    void LevelUp()
    {
        level++;
        currentExp -= expToNextLevel;

        CalculateNextLevelExp();

        Debug.Log("LEVEL UP! Chúc mừng bạn đạt cấp " + level);

        if (levelUpManager != null)
        {
            levelUpManager.TriggerLevelUp();
        }

        // Cập nhật lại UI sau khi lên cấp
        UpdateExpUI();
    }

    void CalculateNextLevelExp()
    {
        expToNextLevel = Mathf.RoundToInt(10f * Mathf.Pow(level, 1.5f));
    }

    public void UpgradeMaxHP(int amount)
    {
        if (playerHealth != null)
        {
            playerHealth.maxHealth += amount;
            playerHealth.currentHealth += amount;
        }
    }

    // THÊM MỚI: Hàm chuyên xử lý việc hiển thị chữ và độ dài thanh màu xanh
    void UpdateExpUI()
    {
        // 1. Cập nhật chữ Cấp độ (Level)
        if (levelText != null)
        {
            levelText.text = level.ToString();
        }

        // 2. Cập nhật chữ EXP (Ví dụ: 5 / 10 XP (50%))
        if (expText != null)
        {
            float percent = (float)currentExp / expToNextLevel * 100f;
            expText.text = $"{currentExp} / {expToNextLevel} XP ({percent:F0}%)";
        }

        // 3. Cập nhật độ dài của thanh màu xanh lá (từ 0.0 đến 1.0)
        if (expBarFill != null)
        {
            expBarFill.fillAmount = (float)currentExp / expToNextLevel;
        }
    }
}