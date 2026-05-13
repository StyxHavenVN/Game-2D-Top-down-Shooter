using UnityEngine;

public class EnemyFollow : MonoBehaviour
{
    [Header("Dữ liệu Quái Cận Chiến (Kéo EnemyData vào)")]
    public EnemyData enemyData;

    private Transform player;
    private Rigidbody2D rb;
    private Vector2 movement;
    private SpriteRenderer spriteRenderer;
    
    // Chỉ số nội tại (đọc từ Data)
    private float moveSpeed = 3f;
    private float damage = 1f;
    private float detectionRange = 15f;
    private float attackCooldown = 0.5f; // Thời gian nghỉ giữa 2 lần gây sát thương

    // Bộ đếm cooldown sát thương — NGĂN VIỆC GÂY DAMAGE MỖI FRAME
    private float attackTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Đọc thông số từ thẻ Data
        if (enemyData != null)
        {
            moveSpeed = enemyData.moveSpeed;
            damage = enemyData.damage;
            detectionRange = enemyData.detectionRange > 0 ? enemyData.detectionRange : 15f;
            attackCooldown = enemyData.attackCooldown > 0 ? enemyData.attackCooldown : 0.5f;
        }

        FindPlayer();
    }

    void OnEnable()
    {
        // Khi lấy quái từ Pool ra, tìm lại Player (Player có thể đã thay đổi)
        FindPlayer();
        attackTimer = 0f;
    }

    private void FindPlayer()
    {
        if (player == null)
        {
            GameObject pObj = GameObject.FindWithTag("Player");
            if (pObj == null) pObj = GameObject.Find("Player");
            if (pObj != null) player = pObj.transform;
        }
    }

    void Update()
    {
        if (player == null) return;

        // Giảm bộ đếm cooldown sát thương
        if (attackTimer > 0f)
            attackTimer -= Time.deltaTime;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Chỉ đuổi khi Player nằm trong tầm phát hiện
        if (distanceToPlayer <= detectionRange)
        {
            Vector3 direction = player.position - transform.position;
            direction.Normalize();
            movement = direction;

            // Lật sprite theo hướng di chuyển (quái nhìn sang trái/phải)
            if (spriteRenderer != null)
            {
                if (direction.x > 0.1f)
                    spriteRenderer.flipX = false;
                else if (direction.x < -0.1f)
                    spriteRenderer.flipX = true;
            }
        }
        else
        {
            // Ngoài tầm phát hiện → đứng yên
            movement = Vector2.zero;
        }
    }

    void FixedUpdate()
    {
        if (movement.sqrMagnitude > 0.01f)
        {
            rb.MovePosition((Vector2)transform.position + (movement * moveSpeed * Time.fixedDeltaTime));
        }
    }
    
    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.name == "Player")
        {
            // CHỈ GÂY SÁT THƯƠNG KHI ĐÃ HẾT COOLDOWN
            if (attackTimer <= 0f)
            {
                Health playerHealth = collision.gameObject.GetComponent<Health>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(Mathf.RoundToInt(damage));
                }
                attackTimer = attackCooldown; // Reset cooldown
            }
        }
    }
}