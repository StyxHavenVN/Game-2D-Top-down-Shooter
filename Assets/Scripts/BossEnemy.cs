using UnityEngine;

public class BossEnemy : EnemyBase
{
    [Header("Boss Contact Attack")]
    public int contactDamage = 15;
    public float damageCooldown = 0.8f;
    private float nextDamageTime;

    [Header("Bắn thường")]
    public GameObject bossBulletPrefab;
    public Transform firePoint;
    public float normalShotCooldown = 1.5f;
    public float normalBulletSpeed = 10f;
    private float nextNormalShotTime;

    [Header("Hồi máu")]
    public int healAmount = 20;
    public float healCooldown = 5f;
    public int maxHealTimes = 5;
    private int currentHealTimes = 0;
    private float nextHealTime;

    [Header("Sinh Mini Enemy")]
    public GameObject miniEnemyPrefab;
    public float summonCooldown = 7f;
    public int summonCount = 2;
    public float summonRadius = 2f;
    private float nextSummonTime;

    [Header("Bắn vòng tròn")]
    public float circleShotCooldown = 6f;
    public int circleBulletCount = 12;
    public float circleBulletSpeed = 8f;
    private float nextCircleShotTime;

    protected override void Start()
    {
        base.Start();

        enemyName = "Boss";

        nextNormalShotTime = Time.time + 1f;
        nextHealTime = Time.time + healCooldown;
        nextSummonTime = Time.time + summonCooldown;
        nextCircleShotTime = Time.time + circleShotCooldown;
    }

    protected override void EnemyUpdate()
    {
        if (player == null || rb == null) return;

        FlipToPlayer();

        HandleNormalShot();
        HandleHeal();
        HandleSummonMiniEnemy();
        HandleCircleShot();
    }

    void FixedUpdate()
    {
        if (isDead) return;

        MoveTowardPlayer();
    }

    void HandleNormalShot()
    {
        if (Time.time < nextNormalShotTime) return;

        ShootAtPlayer();

        nextNormalShotTime = Time.time + normalShotCooldown;
    }

    void HandleHeal()
    {
        if (Time.time < nextHealTime) return;
        if (currentHealTimes >= maxHealTimes) return;
        if (currentHealth >= maxHealth) return;

        currentHealth += healAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        currentHealTimes++;

        UpdateHealthBar();

        Debug.Log("[Boss] Hồi máu: +" + healAmount + ". Máu hiện tại: " + currentHealth + " / " + maxHealth);

        nextHealTime = Time.time + healCooldown;
    }

    void HandleSummonMiniEnemy()
    {
        if (Time.time < nextSummonTime) return;

        SummonMiniEnemies();

        nextSummonTime = Time.time + summonCooldown;
    }

    void HandleCircleShot()
    {
        if (Time.time < nextCircleShotTime) return;

        ShootCircle();

        nextCircleShotTime = Time.time + circleShotCooldown;
    }

    void ShootAtPlayer()
    {
        if (bossBulletPrefab == null)
        {
            Debug.LogWarning("[Boss] Chưa kéo Boss Bullet Prefab.");
            return;
        }

        if (player == null) return;

        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;

        Vector2 direction = ((Vector2)player.position - (Vector2)spawnPos).normalized;

        if (direction == Vector2.zero)
        {
            direction = Vector2.right;
        }

        SpawnBullet(spawnPos, direction, normalBulletSpeed);
    }

    void ShootCircle()
    {
        if (bossBulletPrefab == null)
        {
            Debug.LogWarning("[Boss] Chưa kéo Boss Bullet Prefab.");
            return;
        }

        if (circleBulletCount <= 0)
        {
            circleBulletCount = 8;
        }

        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;

        float angleStep = 360f / circleBulletCount;

        for (int i = 0; i < circleBulletCount; i++)
        {
            float angle = angleStep * i;
            float rad = angle * Mathf.Deg2Rad;

            Vector2 direction = new Vector2(
                Mathf.Cos(rad),
                Mathf.Sin(rad)
            ).normalized;

            SpawnBullet(spawnPos, direction, circleBulletSpeed);
        }

        Debug.Log("[Boss] Bắn đạn vòng tròn.");
    }

    void SpawnBullet(Vector3 spawnPos, Vector2 direction, float speed)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        GameObject bullet = Instantiate(
            bossBulletPrefab,
            spawnPos,
            Quaternion.Euler(0, 0, angle)
        );

        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();

        if (bulletRb != null)
        {
            bulletRb.gravityScale = 0f;
            bulletRb.linearVelocity = direction * speed;
        }
        else
        {
            Debug.LogWarning("[Boss] Bullet thiếu Rigidbody2D.");
        }
    }

    void SummonMiniEnemies()
    {
        if (miniEnemyPrefab == null)
        {
            Debug.LogWarning("[Boss] Chưa kéo Mini Enemy Prefab.");
            return;
        }

        for (int i = 0; i < summonCount; i++)
        {
            Vector2 randomDir = Random.insideUnitCircle.normalized;

            if (randomDir == Vector2.zero)
            {
                randomDir = Vector2.right;
            }

            Vector3 spawnPos = transform.position + (Vector3)(randomDir * summonRadius);

            Instantiate(
                miniEnemyPrefab,
                spawnPos,
                Quaternion.identity
            );
        }

        Debug.Log("[Boss] Sinh " + summonCount + " mini enemy.");
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (isDead) return;
        if (!collision.gameObject.CompareTag("Player")) return;
        if (Time.time < nextDamageTime) return;

        Health playerHealth = collision.gameObject.GetComponent<Health>();

        if (playerHealth == null)
        {
            playerHealth = collision.gameObject.GetComponentInParent<Health>();
        }

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(contactDamage);
            nextDamageTime = Time.time + damageCooldown;
        }
    }

    protected override void Die()
    {
        Debug.Log("Boss đã chết!");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ShowVictory();
        }

        base.Die();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayDefaultMusic();
        }
    }
}