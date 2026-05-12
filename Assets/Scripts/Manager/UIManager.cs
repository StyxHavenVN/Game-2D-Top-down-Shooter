using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UIManager - Quản lý toàn bộ giao diện người dùng (HUD) tập trung tại một nơi.
/// Áp dụng Singleton Pattern để các script khác dễ dàng gọi tới.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("HUD - Thanh Máu (HP)")]
    public Image hpBarFill;
    public TextMeshProUGUI hpText;

    [Header("HUD - Thanh Kinh Nghiệm (EXP)")]
    public Image expBarFill;
    public TextMeshProUGUI expText;
    public TextMeshProUGUI levelText;

    [Header("HUD - Kill Count")]
    public TextMeshProUGUI killCountText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Cập nhật thanh máu và số máu
    /// </summary>
    public void UpdateHP(int currentHP, int maxHP)
    {
        if (hpBarFill != null)
            hpBarFill.fillAmount = (float)currentHP / maxHP;

        if (hpText != null)
            hpText.text = $"{currentHP} / {maxHP}";
    }

    /// <summary>
    /// Cập nhật thanh EXP và Cấp độ
    /// </summary>
    public void UpdateEXP(int currentEXP, int expToNextLevel, int currentLevel)
    {
        if (expBarFill != null)
            expBarFill.fillAmount = (float)currentEXP / expToNextLevel;

        if (expText != null)
        {
            float percent = (float)currentEXP / expToNextLevel * 100f;
            expText.text = $"{currentEXP} / {expToNextLevel} XP ({percent:F0}%)";
        }

        if (levelText != null)
            levelText.text = currentLevel.ToString();
    }

    /// <summary>
    /// Cập nhật số lượng quái đã giết
    /// </summary>
    public void UpdateKillCount(int currentKills, int killThreshold, bool isBossSpawned)
    {
        if (killCountText != null)
        {
            if (isBossSpawned)
                killCountText.text = "⚠ BOSS!";
            else
                killCountText.text = $"Kills: {currentKills} / {killThreshold}";
        }
    }
}
