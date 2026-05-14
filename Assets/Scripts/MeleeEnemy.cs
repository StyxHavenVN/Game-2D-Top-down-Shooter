using UnityEngine;

public class MeleeEnemy : EnemyBase
{
    [Header("Né vật cản")]
    public LayerMask obstacleLayer;
    public float obstacleCheckDistance = 2f;
    public float obstacleCheckRadius = 0.25f;
    public float avoidTime = 0.7f;

    [Header("Tách khỏi quái khác")]
    public LayerMask enemyLayer;
    public float separationRadius = 1f;
    public float separationForce = 2f;

    [Header("Cận chiến")]
    public int contactDamage = 5;
    public float damageCooldown = 0.5f;

    private float nextDamageTime;

    private Vector2 movement;
    private Vector2 avoidDirection;
    private float avoidTimer;

    protected override void EnemyUpdate()
    {
        if (player == null || rb == null)
        {
            movement = Vector2.zero;
            return;
        }

        FlipToPlayer();

        Vector2 directionToPlayer = ((Vector2)player.position - rb.position).normalized;

        // Nếu đang trong trạng thái né vật cản thì tiếp tục né một lúc
        if (avoidTimer > 0f)
        {
            avoidTimer -= Time.deltaTime;

            if (!IsBlocked(avoidDirection))
            {
                movement = CombineWithSeparation(avoidDirection);
                return;
            }
        }

        // Nếu đường tới Player không bị chắn thì đi thẳng
        if (!IsBlocked(directionToPlayer))
        {
            movement = CombineWithSeparation(directionToPlayer);
            return;
        }

        // Nếu bị chắn thì tìm hướng đi vòng
        avoidDirection = FindBestAvoidDirection(directionToPlayer);
        avoidTimer = avoidTime;

        movement = CombineWithSeparation(avoidDirection);
    }

    void FixedUpdate()
    {
        if (isDead) return;
        if (rb == null) return;

        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }

    Vector2 FindBestAvoidDirection(Vector2 directionToPlayer)
    {
        float[] angles =
        {
            30f, -30f,
            45f, -45f,
            60f, -60f,
            90f, -90f,
            120f, -120f,
            150f, -150f,
            180f
        };

        Vector2 bestDirection = Vector2.zero;
        float bestScore = float.MaxValue;

        foreach (float angle in angles)
        {
            Vector2 testDirection = RotateVector(directionToPlayer, angle);

            if (IsBlocked(testDirection))
                continue;

            Vector2 testPosition = rb.position + testDirection;
            float distanceToPlayer = Vector2.Distance(testPosition, player.position);

            if (distanceToPlayer < bestScore)
            {
                bestScore = distanceToPlayer;
                bestDirection = testDirection;
            }
        }

        if (bestDirection != Vector2.zero)
        {
            return bestDirection.normalized;
        }

        // Nếu mọi hướng đều bị chắn, thử đi ngang để thoát kẹt
        Vector2 sideDirection = RotateVector(directionToPlayer, 90f);

        if (!IsBlocked(sideDirection))
            return sideDirection;

        sideDirection = RotateVector(directionToPlayer, -90f);

        if (!IsBlocked(sideDirection))
            return sideDirection;

        return Vector2.zero;
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
        if (obstacleLayer.value == 0) return false;

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
        if (enemyLayer.value == 0) return Vector2.zero;

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
            {
                result += away.normalized / away.magnitude;
            }
        }

        return result.normalized;
    }

    Vector2 RotateVector(Vector2 vector, float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;

        float sin = Mathf.Sin(radians);
        float cos = Mathf.Cos(radians);

        float x = vector.x * cos - vector.y * sin;
        float y = vector.x * sin + vector.y * cos;

        return new Vector2(x, y).normalized;
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

    void OnDrawGizmosSelected()
    {
        if (rb == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(rb.position + movement * obstacleCheckDistance, obstacleCheckRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(rb.position, separationRadius);
    }
}