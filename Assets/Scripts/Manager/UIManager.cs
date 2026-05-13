using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

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

    [Header("HUD - Wave Info")]
    [Tooltip("Text hiển thị thông tin Wave (góc trên trái)")]
    public TextMeshProUGUI waveText;

    [Header("HUD - Boss Victory (Tùy chọn)")]
    [Tooltip("Text hiển thị thông báo chiến thắng Boss (để trống nếu dùng waveText)")]
    public TextMeshProUGUI bossVictoryText;

    private Coroutine victoryCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        // Ẩn text chiến thắng Boss khi bắt đầu
        if (bossVictoryText != null)
            bossVictoryText.gameObject.SetActive(false);
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
    /// Cập nhật số lượng quái đã giết (đơn giản hóa — không còn threshold)
    /// </summary>
    public void UpdateKillCount(int currentKills)
    {
        if (killCountText != null)
        {
            killCountText.text = $"Kills: {currentKills}";
        }
    }

    /// <summary>
    /// Cập nhật thông tin Wave: wave hiện tại, số quái còn sống, trạng thái Boss.
    /// </summary>
    public void UpdateWaveInfo(int wave, int enemiesAlive, bool isBossWave)
    {
        if (waveText != null)
        {
            if (wave == 0)
            {
                waveText.text = "Chuẩn bị...";
            }
            else if (isBossWave)
            {
                waveText.text = $"⚠ WAVE {wave} - BOSS!";
            }
            else
            {
                waveText.text = $"Wave {wave}\nQuái còn: {enemiesAlive}";
            }
        }
    }

    /// <summary>
    /// Hiển thị thông báo chiến thắng Boss — tự ẩn sau 5 giây.
    /// Nếu có bossVictoryText riêng → hiển thị ở đó.
    /// Nếu không → hiển thị tạm trên waveText.
    /// </summary>
    public void ShowBossVictory(int wave)
    {
        // Dừng coroutine cũ nếu đang chạy
        if (victoryCoroutine != null)
            StopCoroutine(victoryCoroutine);

        victoryCoroutine = StartCoroutine(BossVictoryRoutine(wave));
    }

    /// <summary>
    /// Coroutine hiển thị thông báo chiến thắng Boss trong 5 giây.
    /// </summary>
    private IEnumerator BossVictoryRoutine(int wave)
    {
        string victoryMessage = $"★ BOSS DEFEATED! ★\nWave {wave} Complete!";

        // Nếu có text riêng cho victory → hiển thị ở đó
        if (bossVictoryText != null)
        {
            bossVictoryText.gameObject.SetActive(true);
            bossVictoryText.text = victoryMessage;

            // Hiệu ứng nhấp nháy text
            float elapsed = 0f;
            float totalDuration = 5f;
            while (elapsed < totalDuration)
            {
                // Nhấp nháy màu vàng ↔ trắng
                bossVictoryText.color = Mathf.PingPong(Time.time * 3f, 1f) > 0.5f
                    ? Color.yellow
                    : Color.white;
                elapsed += Time.deltaTime;
                yield return null;
            }

            bossVictoryText.gameObject.SetActive(false);
        }
        else if (waveText != null)
        {
            // Fallback: dùng waveText
            string originalText = waveText.text;
            Color originalColor = waveText.color;

            waveText.text = victoryMessage;

            float elapsed = 0f;
            float totalDuration = 5f;
            while (elapsed < totalDuration)
            {
                waveText.color = Mathf.PingPong(Time.time * 3f, 1f) > 0.5f
                    ? Color.yellow
                    : Color.white;
                elapsed += Time.deltaTime;
                yield return null;
            }

            waveText.color = originalColor;
        }

        victoryCoroutine = null;
    }
}
