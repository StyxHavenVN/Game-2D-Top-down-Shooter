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

        float cooldownPercent = playerDash.GetCooldownPercent();

        if (cooldownSlider != null)
            cooldownSlider.value = cooldownPercent;

        if (dashIconImage != null)
            dashIconImage.color = cooldownPercent <= 0f ? readyColor : cooldownColor;

        if (cooldownText != null)
        {
            if (cooldownPercent <= 0f)
                cooldownText.text = "READY";
            else
            {
                float remaining = cooldownPercent * playerDash.dashCooldown;
                cooldownText.text = $"{remaining:F1}s";
            }
        }
    }
}