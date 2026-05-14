using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    [Header("Chỉ số máu")]
    public int maxHealth = 50;
    private int currentHealth;

    [Header("Giao diện Máu")]
    public Image healthFill;

    [Header("Phần thưởng")]
    public int expReward = 10;

    [Tooltip("Kéo Prefab ExpOrb vào đây")]
    public GameObject expOrbPrefab;

    private bool isDead = false;

    void Start()
    {
        if (maxHealth <= 0)
        {
            Debug.LogWarning("[EnemyHealth] maxHealth phải lớn hơn 0. Tự đặt lại thành 50.");
            maxHealth = 50;
        }

        currentHealth = maxHealth;
        UpdateHealthBar();
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;
        if (damage <= 0) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log(gameObject.name + " bị dính đòn! Máu còn: " + currentHealth);

        UpdateHealthBar();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void UpdateHealthBar()
    {
        if (healthFill != null && maxHealth > 0)
        {
            healthFill.fillAmount = (float)currentHealth / maxHealth;
        }
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;

        Debug.Log(gameObject.name + " đã bị tiêu diệt!");

        DropExpOrb();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterKill();
        }

        Destroy(gameObject);
    }

    void DropExpOrb()
    {
        if (expOrbPrefab == null) return;

        GameObject orb = Instantiate(expOrbPrefab, transform.position, Quaternion.identity);

        ExpOrb expOrb = orb.GetComponent<ExpOrb>();
        if (expOrb != null)
        {
            expOrb.expValue = expReward;
        }
    }
} 