using UnityEngine;

public class RangedEnemy : EnemyBase
{
    [Header("Đánh xa")]
    public GameObject enemyBulletPrefab;
    public Transform firePoint;

    [Header("Khoảng cách")]
    public float preferredDistance = 6f;
    public float retreatDistance = 3f;

    [Header("Bắn")]
    public float fireCooldown = 1.5f;
    public float bulletForce = 10f;

    [Header("Né vật cản")]
    public LayerMask obstacleLayer;
    public float obstacleCheckDistance = 2f;
    public float obstacleCheckRadius = 0.25f;
    public float avoidTime = 0.7f;

    [Header("Tách khỏi quái khác")]
    public LayerMask enemyLayer;
    public float separationRadius = 1f;
    public float separationForce = 2f;

    private float nextFireTime;
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

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance > preferredDistance)
        {
            Vector2 directionToPlayer = ((Vector2)player.position - rb.position).normalized;
            movement = GetSmartMoveDirection(directionToPlayer);
        }
        else if (distance < retreatDistance)
        {
            Vector2 directionAway = (rb.position - (Vector2)player.position).normalized;
            movement = GetSmartMoveDirection(directionAway);
        }
        else
        {
            movement = GetSeparationDirection() * separationForce;

            if (movement != Vector2.zero)
                movement = movement.normalized;
        }

        if (Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireCooldown;
        }
    }

    void FixedUpdate()
    {
        if (isDead) return;
        if (rb == null) return;

        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }

    void Shoot()
    {
        if (enemyBulletPrefab == null)
        {
            Debug.LogError("[RangedEnemy] Chưa kéo Enemy Bullet Prefab.");
            return;
        }

        if (player == null) return;

        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;

        Vector2 direction = ((Vector2)player.position - (Vector2)spawnPos).normalized;

        if (direction == Vector2.zero)
        {
            direction = Vector2.right;
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        GameObject bullet = Instantiate(
            enemyBulletPrefab,
            spawnPos,
            Quaternion.Euler(0, 0, angle)
        );

        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();

        if (bulletRb != null)
        {
            bulletRb.gravityScale = 0f;
            bulletRb.linearVelocity = direction * bulletForce;
        }
        else
        {
            Debug.LogError("[RangedEnemy] Enemy bullet thiếu Rigidbody2D.");
        }

        Debug.Log("[RangedEnemy] Đã bắn đạn.");
    }

    Vector2 GetSmartMoveDirection(Vector2 mainDirection)
    {
        if (mainDirection == Vector2.zero)
            return Vector2.zero;

        if (avoidTimer > 0f)
        {
            avoidTimer -= Time.deltaTime;

            if (!IsBlocked(avoidDirection))
                return CombineWithSeparation(avoidDirection);
        }

        if (!IsBlocked(mainDirection))
            return CombineWithSeparation(mainDirection);

        avoidDirection = FindBestAvoidDirection(mainDirection);
        avoidTimer = avoidTime;

        return CombineWithSeparation(avoidDirection);
    }

    Vector2 FindBestAvoidDirection(Vector2 mainDirection)
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
            Vector2 testDirection = RotateVector(mainDirection, angle);

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
            return bestDirection.normalized;

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
}