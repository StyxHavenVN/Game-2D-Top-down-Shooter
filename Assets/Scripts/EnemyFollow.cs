using UnityEngine;

public class EnemyFollow : MonoBehaviour
{
    [Header("Mục tiêu")]
    public Transform player;

    [Header("Di chuyển")]
    public float moveSpeed = 3f;

    [Header("Né vật cản")]
    public LayerMask obstacleLayer;
    public float obstacleCheckDistance = 1.5f;
    public float obstacleCheckRadius = 0.25f;
    public float avoidTime = 0.7f;

    [Header("Tách khỏi quái khác")]
    public LayerMask enemyLayer;
    public float separationRadius = 0.7f;
    public float separationForce = 1.2f;

    [Header("Gây sát thương khi chạm Player")]
    public int contactDamage = 5;
    public float damageCooldown = 0.5f;

    private Rigidbody2D rb;
    private Vector2 movement;
    private Vector2 avoidDirection;
    private float avoidTimer;
    private float nextDamageTime;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (player == null)
        {
            GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");

            if (foundPlayer != null)
                player = foundPlayer.transform;
            else
                Debug.LogError("[EnemyFollow] Không tìm thấy Player. Hãy gắn Tag Player cho nhân vật.");
        }

        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
        }
    }

    void Update()
    {
        if (player == null || rb == null)
        {
            movement = Vector2.zero;
            return;
        }

        Vector2 toPlayer = ((Vector2)player.position - rb.position).normalized;

        if (avoidTimer > 0f)
        {
            avoidTimer -= Time.deltaTime;

            if (!IsBlocked(avoidDirection))
            {
                movement = CombineWithSeparation(avoidDirection);
                return;
            }
        }

        if (!IsBlocked(toPlayer))
        {
            movement = CombineWithSeparation(toPlayer);
            return;
        }

        avoidDirection = FindAroundObstacleDirection(toPlayer);
        avoidTimer = avoidTime;

        movement = CombineWithSeparation(avoidDirection);
    }

    void FixedUpdate()
    {
        if (rb == null) return;

        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }

    Vector2 FindAroundObstacleDirection(Vector2 toPlayer)
    {
        float[] angles =
        {
            45f, -45f,
            70f, -70f,
            90f, -90f,
            120f, -120f,
            150f, -150f,
            180f
        };

        Vector2 bestDir = Vector2.zero;
        float bestScore = float.MaxValue;

        foreach (float angle in angles)
        {
            Vector2 dir = RotateVector(toPlayer, angle);

            if (IsBlocked(dir))
                continue;

            Vector2 testPos = rb.position + dir;
            float score = Vector2.Distance(testPos, player.position);

            if (score < bestScore)
            {
                bestScore = score;
                bestDir = dir;
            }
        }

        if (bestDir != Vector2.zero)
            return bestDir.normalized;

        // Nếu mọi hướng đều bị chặn, đi ngang để thoát kẹt
        return RotateVector(toPlayer, 90f).normalized;
    }

    Vector2 CombineWithSeparation(Vector2 mainDirection)
    {
        Vector2 separation = GetSeparationDirection();
        Vector2 finalDirection = mainDirection + separation * separationForce;

        if (finalDirection == Vector2.zero)
            return mainDirection;

        return finalDirection.normalized;
    }

    bool IsBlocked(Vector2 direction)
    {
        RaycastHit2D hit = Physics2D.CircleCast(
            rb.position,
            obstacleCheckRadius,
            direction,
            obstacleCheckDistance,
            obstacleLayer
        );

        return hit.collider != null;
    }

    Vector2 GetSeparationDirection()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            rb.position,
            separationRadius,
            enemyLayer
        );

        Vector2 result = Vector2.zero;

        foreach (Collider2D hit in hits)
        {
            if (hit.gameObject == gameObject) continue;

            Vector2 away = rb.position - (Vector2)hit.transform.position;

            if (away.sqrMagnitude > 0.001f)
                result += away.normalized / away.magnitude;
        }

        return result.normalized;
    }

    Vector2 RotateVector(Vector2 vector, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;

        float sin = Mathf.Sin(rad);
        float cos = Mathf.Cos(rad);

        float x = vector.x * cos - vector.y * sin;
        float y = vector.x * sin + vector.y * cos;

        return new Vector2(x, y).normalized;
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;
        if (Time.time < nextDamageTime) return;

        Health playerHealth = collision.gameObject.GetComponent<Health>();

        if (playerHealth == null)
            playerHealth = collision.gameObject.GetComponentInParent<Health>();

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(contactDamage);
            nextDamageTime = Time.time + damageCooldown;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (rb == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(rb.position + movement * obstacleCheckDistance, obstacleCheckRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(rb.position, separationRadius);
    }
}