using System.Collections;
using UnityEngine;

public class BossController : MonoBehaviour
{
    public enum BossState { Idle, Chasing, SkillSpreadShoot, SkillDash }
    
    [Header("Dữ liệu Boss (Kéo Boss_Data vào đây)")]
    public EnemyData enemyData;

    [Header("Cài đặt chung")]
    public BossState currentState = BossState.Chasing;
    
    private float moveSpeed = 1.5f;
    private float stateCooldown = 3f; 
    private float damage = 10f;
    private float attackCooldown = 0.8f; // Cooldown va chạm gây sát thương
    private float contactAttackTimer = 0f;
    
    [Header("Cài đặt Kỹ năng: Bắn chùm (Spread Shoot)")]
    public GameObject bossBulletPrefab;
    public Transform firePoint;
    public int bulletAmount = 8; 
    private float bulletForce = 8f;

    [Header("Cài đặt Kỹ năng: Lao tới (Dash)")]
    public float dashSpeed = 15f;
    public float dashDuration = 0.5f;

    private Transform player;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private bool isExecutingSkill = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Đọc thông số từ thẻ Data
        if (enemyData != null)
        {
            moveSpeed = enemyData.moveSpeed;
            stateCooldown = enemyData.attackCooldown;
            bulletForce = enemyData.bulletSpeed;
            damage = enemyData.damage;
        }
        
        GameObject p = GameObject.FindWithTag("Player");
        if (p == null) p = GameObject.Find("Player");
        if (p != null) player = p.transform;

        StartCoroutine(BossThinkRoutine());
    }

    void Update()
    {
        if (player == null || isExecutingSkill) return;

        // Giảm cooldown sát thương va chạm
        if (contactAttackTimer > 0f)
            contactAttackTimer -= Time.deltaTime;

        if (currentState == BossState.Chasing)
        {
            Vector2 targetPos = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
            rb.MovePosition(targetPos);
        }

        // Lật sprite theo hướng di chuyển
        if (spriteRenderer != null && player != null)
        {
            float dirX = player.position.x - transform.position.x;
            if (dirX > 0.1f) spriteRenderer.flipX = false;
            else if (dirX < -0.1f) spriteRenderer.flipX = true;
        }
    }

    IEnumerator BossThinkRoutine()
    {
        while (true)
        {
            currentState = BossState.Chasing;
            yield return new WaitForSeconds(stateCooldown);

            isExecutingSkill = true;
            int randomSkill = Random.Range(0, 2);

            if (randomSkill == 0)
            {
                currentState = BossState.SkillSpreadShoot;
                yield return StartCoroutine(SpreadShootRoutine());
            }
            else if (randomSkill == 1)
            {
                currentState = BossState.SkillDash;
                yield return StartCoroutine(DashRoutine());
            }

            isExecutingSkill = false;
        }
    }

    IEnumerator SpreadShootRoutine()
    {
        rb.linearVelocity = Vector2.zero; 
        float angleStep = 360f / bulletAmount;
        float angle = 0f;

        for (int i = 0; i < bulletAmount; i++)
        {
            float dirX = transform.position.x + Mathf.Sin((angle * Mathf.PI) / 180f);
            float dirY = transform.position.y + Mathf.Cos((angle * Mathf.PI) / 180f);
            Vector3 bulletMoveVector = new Vector3(dirX, dirY, 0f);
            Vector2 bulletDir = (bulletMoveVector - transform.position).normalized;

            if (bossBulletPrefab != null && firePoint != null)
            {
                GameObject bullet = Instantiate(bossBulletPrefab, firePoint.position, Quaternion.identity);
                bullet.GetComponent<Rigidbody2D>().AddForce(bulletDir * bulletForce, ForceMode2D.Impulse);
            }
            angle += angleStep;
        }
        yield return new WaitForSeconds(1f); 
    }

    IEnumerator DashRoutine()
    {
        if (player == null) yield break;

        Vector2 dashDirection = (player.position - transform.position).normalized;
        float startTime = Time.time;
        while (Time.time < startTime + dashDuration)
        {
            rb.linearVelocity = dashDirection * dashSpeed;
            yield return null;
        }
        rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(0.5f); 
    }
    
    // Boss chạm vào người chơi gây sát thương — CÓ COOLDOWN
    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.name == "Player")
        {
            if (contactAttackTimer <= 0f)
            {
                Health playerHealth = collision.gameObject.GetComponent<Health>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(Mathf.RoundToInt(damage)); 
                }
                contactAttackTimer = attackCooldown;
            }
        }
    }
}
