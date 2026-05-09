using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// DashCooldownUI - Hiển thị cooldown Dash lên UI.
/// Attach vào Canvas, kéo reference PlayerDash vào.
/// 
/// Setup trong Hierarchy:
///   Canvas
///   └─ DashCooldownPanel
///      ├─ DashIcon (Image)
///      ├─ CooldownBar (Slider)
///      └─ CooldownText (TextMeshProUGUI) - hiện "READY" hoặc "2.1s"
/// </summary>
public class DashCooldownUI : MonoBehaviour
{
    [Header("References")]
    public PlayerDash playerDash;
    public Slider cooldownSlider;
    public Image dashIconImage;
    public TextMeshProUGUI cooldownText;

    [Header("Colors")]
    public Color readyColor = new Color(0.3f, 0.8f, 1f);
    public Color cooldownColor = new Color(0.4f, 0.4f, 0.4f);

    void Start()
    {
        if (playerDash == null)
            playerDash = FindAnyObjectByType<PlayerDash>();

        if (cooldownSlider != null)
            cooldownSlider.maxValue = 1f;
    }

    void Update()
    {
        if (playerDash == null) return;

        float progress = 1f - playerDash.GetCooldownPercent(); // 0 = ready, 1 = full cooldown

        // Slider (1 = cooldown đầy, 0 = sẵn sàng — fill từ phải sang trái)
        if (cooldownSlider != null)
            cooldownSlider.value = progress;

        // Icon màu
        if (dashIconImage != null)
            dashIconImage.color = progress <= 0f ? readyColor : cooldownColor;

        // Text
        if (cooldownText != null)
        {
            if (progress <= 0f)
                cooldownText.text = "READY";
            else
            {
                float remaining = progress * playerDash.dashCooldown;
                cooldownText.text = $"{remaining:F1}s";
            }
        }
    }
}