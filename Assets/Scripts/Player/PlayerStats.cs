using UnityEngine;
using UnityEngine.UI; 
using TMPro;        

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
    public TextMeshProUGUI levelText; 
    public Image expBarFill;          
    public TextMeshProUGUI expText;   

    [Header("Tham chiếu Script khác")]
    public LevelUpManager levelUpManager;
    private Health playerHealth;

    void Start()
    {
        playerHealth = GetComponent<Health>();
        CalculateNextLevelExp();

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

        UpdateExpUI();
    }

    void CalculateNextLevelExp()
    {
        expToNextLevel = Mathf.RoundToInt(10f * Mathf.Pow(level, 1.5f));
    }

    // --- CÁC HÀM XỬ LÝ NÂNG CẤP TỪ LEVEL UP MANAGER ---

    public void UpgradeMaxHP(int amount)
    {
        if (playerHealth != null)
        {
            playerHealth.maxHealth += amount;
            playerHealth.currentHealth += amount;

            if (playerHealth.currentHealth > playerHealth.maxHealth)
            {
                playerHealth.currentHealth = playerHealth.maxHealth;
            }
            playerHealth.UpdateHealthUI(); 
        }
    }

    public void UpgradeATK(float amount)
    {
        atkMultiplier += amount;
        Debug.Log($"[Nâng cấp] Sát thương tăng thêm {amount * 100}%. Tổng buff: +{atkMultiplier * 100}%");
    }

    public void UpgradeDEF(float amount)
    {
        defMultiplier += amount;
        Debug.Log($"[Nâng cấp] Giảm sát thương nhận vào thêm {amount * 100}%. Tổng buff: +{defMultiplier * 100}%");
    }
    void UpdateExpUI()
    {
        if (levelText != null)
        {
            levelText.text = level.ToString();
        }

        if (expText != null)
        {
            float percent = (float)currentExp / expToNextLevel * 100f;
            expText.text = $"{currentExp} / {expToNextLevel} XP ({percent:F0}%)";
        }

        if (expBarFill != null)
        {
            expBarFill.fillAmount = (float)currentExp / expToNextLevel;
        }
    }
}