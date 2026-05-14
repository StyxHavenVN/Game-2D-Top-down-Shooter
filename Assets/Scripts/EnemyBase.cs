using UnityEngine;
using UnityEngine.UI;

public abstract class EnemyBase : MonoBehaviour
{
    [Header("Thông tin Enemy")]
    public string enemyName = "Enemy";

    [Header("Máu")]
    public int maxHealth = 50;
    protected int currentHealth;
    protected bool isDead = false;

    [Header("Giao diện Máu")]
    public Image healthFill;

    [Header("Damage Popup")]
    public DamagePopup damagePopupPrefab;
    public Vector3 damagePopupOffset = new Vector3(0f, 0.8f, 0f);
    public Color damagePopupColor = Color.yellow;

    [Header("Phần thưởng")]
    public int expReward = 10;
    public GameObject expOrbPrefab;

    [Header("Di chuyển")]
    public float moveSpeed = 3f;
    protected Transform player;
    protected Rigidbody2D rb;

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");
        if (foundPlayer != null)
        {
            player = foundPlayer.transform;
        }
        else
        {
            Debug.LogWarning("[" + enemyName + "] Không tìm thấy Player.");
        }

        if (maxHealth <= 0)
        {
            Debug.LogWarning("[" + enemyName + "] maxHealth phải lớn hơn 0. Tự đặt lại thành 50.");
            maxHealth = 50;
        }

        currentHealth = maxHealth;
        UpdateHealthBar();

        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
        }
    }

    protected virtual void Update()
    {
        if (isDead) return;

        EnemyUpdate();
    }

    protected abstract void EnemyUpdate();

    public virtual void TakeDamage(int damage)
    {
        if (isDead) return;
        if (damage <= 0) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        ShowDamagePopup(damage);

        Debug.Log(enemyName + " mất " + damage + " máu. Máu còn: " + currentHealth);

        UpdateHealthBar();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected virtual void ShowDamagePopup(int damage)
    {
        if (damagePopupPrefab == null) return;

        Vector3 spawnPos = transform.position + damagePopupOffset;

        DamagePopup popup = Instantiate(
            damagePopupPrefab,
            spawnPos,
            Quaternion.identity
        );

        popup.Setup("-" + damage, damagePopupColor);
    }

    protected virtual void UpdateHealthBar()
    {
        if (healthFill != null && maxHealth > 0)
        {
            healthFill.fillAmount = (float)currentHealth / maxHealth;
        }
    }

    protected virtual void Die()
    {
        if (isDead) return;

        isDead = true;

        DropExp();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterKill();
        }

        Destroy(gameObject);
    }

    protected virtual void DropExp()
    {
        if (expOrbPrefab == null) return;

        GameObject orb = Instantiate(expOrbPrefab, transform.position, Quaternion.identity);

        ExpOrb expOrb = orb.GetComponent<ExpOrb>();
        if (expOrb != null)
        {
            expOrb.expValue = expReward;
        }
    }

    protected void MoveTowardPlayer()
    {
        if (player == null || rb == null) return;

        Vector2 direction = ((Vector2)player.position - rb.position).normalized;
        rb.MovePosition(rb.position + direction * moveSpeed * Time.fixedDeltaTime);
    }

    protected void FlipToPlayer()
    {
        if (player == null) return;

        float directionX = player.position.x - transform.position.x;

        if (directionX > 0.05f)
        {
            transform.localScale = new Vector3(
                Mathf.Abs(transform.localScale.x),
                transform.localScale.y,
                transform.localScale.z
            );
        }
        else if (directionX < -0.05f)
        {
            transform.localScale = new Vector3(
                -Mathf.Abs(transform.localScale.x),
                transform.localScale.y,
                transform.localScale.z
            );
        }
    }
}