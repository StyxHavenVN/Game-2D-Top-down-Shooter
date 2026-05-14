using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class Health : MonoBehaviour
{
    [Header("Chỉ số máu")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Giao Diện Thanh Máu")]
    public Image healthFillImage;
    public TextMeshProUGUI hpText;

    [Header("Damage Popup")]
    public DamagePopup damagePopupPrefab;
    public Vector3 damagePopupOffset = new Vector3(0f, 0.8f, 0f);

    [Header("Quản lý Game Over")]
    public GameOverManager gameOverManager;

    private bool isInvincible = false;
    private bool isDead = false;

    private PlayerStats playerStats;

    void Start()
    {
        if (maxHealth <= 0)
        {
            Debug.LogWarning("[Health] maxHealth phải lớn hơn 0. Tự đặt lại thành 100.");
            maxHealth = 100;
        }

        currentHealth = maxHealth;

        playerStats = GetComponent<PlayerStats>();

        UpdateHealthUI();
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;
        if (damage <= 0) return;

        if (isInvincible)
        {
            Debug.Log("[Health] Đang i-frames — miễn nhiễm sát thương!");
            return;
        }

        if (playerStats != null && playerStats.defMultiplier > 0)
        {
            // Tính lượng sát thương được cản lại (Sát thương gốc * Hệ số buff)
            float blockedDamage = damage * playerStats.defMultiplier;

            float finalDamageFloat = damage - blockedDamage;

            damage = Mathf.Max(1, Mathf.RoundToInt(finalDamageFloat));
        }
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        ShowDamagePopup(damage);

        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void ShowDamagePopup(int damage)
    {
        if (damagePopupPrefab == null) return;

        Vector3 spawnPos = transform.position + damagePopupOffset;

        DamagePopup popup = Instantiate(
            damagePopupPrefab,
            spawnPos,
            Quaternion.identity
        );

        popup.Setup("-" + damage, Color.red);
    }

    private void Die()
    {
        if (isDead) return;

        isDead = true;
        currentHealth = 0;
        UpdateHealthUI();

        Debug.Log("[Health] Player đã chết!");

        if (gameOverManager != null)
        {
            gameOverManager.ShowGameOver();
        }
        else
        {
            Debug.LogWarning("[Health] Chưa gán GameOverManager.");
        }
    }

    public void SetInvincible(bool value)
    {
        if (isDead) return;

        isInvincible = value;
        Debug.Log($"[Health] I-frames: {(value ? "BẬT" : "TẮT")}");
    }

    public void Heal(int amount)
    {
        if (isDead) return;
        if (amount <= 0) return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateHealthUI();
    }

    public bool IsInvincible()
    {
        return isInvincible;
    }

    public bool IsDead()
    {
        return isDead;
    }

    public void UpdateHealthUI()
    {
        if (maxHealth <= 0) return;

        if (healthFillImage != null)
        {
            healthFillImage.fillAmount = (float)currentHealth / maxHealth;
        }

        if (hpText != null)
        {
            hpText.text = currentHealth + " / " + maxHealth;
        }
    }
}