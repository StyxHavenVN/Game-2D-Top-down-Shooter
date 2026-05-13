using UnityEngine;

public class RangedEnemy : MonoBehaviour
{
    public enum EnemyState { Idle, Chase, Attack }
    
    [Header("Dữ liệu Quái Bắn Xa (Kéo EnemyData vào)")]
    public EnemyData enemyData;

    [Header("Trạng thái hiện tại")]
    public EnemyState currentState = EnemyState.Idle;

    private float moveSpeed = 2f;
    private float stoppingDistance = 5f; 
    private float retreatDistance = 3f;  
    private float fireRate = 1.5f; 
    private float bulletSpeed = 10f;
    private float detectionRange = 15f;

    [Header("Vũ khí")]
    public GameObject enemyBulletPrefab;
    public Transform firePoint;

    private Transform player;
    private float nextFireTime;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        InitializeFromData();
    }

    void OnEnable()
    {
        // Khi lấy từ Pool, cần tìm lại Player và reset trạng thái
        FindPlayer();
        currentState = EnemyState.Idle;
        nextFireTime = 0f;
    }

    private void InitializeFromData()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Đọc thông số từ thẻ Data
        if (enemyData != null)
        {
            moveSpeed = enemyData.moveSpeed;
            stoppingDistance = enemyData.preferredDistance;
            retreatDistance = stoppingDistance - 2f;
            fireRate = enemyData.attackCooldown;
            bulletSpeed = enemyData.bulletSpeed;
            detectionRange = enemyData.detectionRange > 0 ? enemyData.detectionRange : 15f;
        }

        FindPlayer();
    }

    private void FindPlayer()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj == null) playerObj = GameObject.Find("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            currentState = EnemyState.Chase;
        }
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Ngoài tầm phát hiện → đứng yên
        if (distanceToPlayer > detectionRange)
        {
            currentState = EnemyState.Idle;
            return;
        }

        // Lật sprite theo hướng nhìn về Player
        if (spriteRenderer != null)
        {
            float dirX = player.position.x - transform.position.x;
            if (dirX > 0.1f) spriteRenderer.flipX = false;
            else if (dirX < -0.1f) spriteRenderer.flipX = true;
        }

        if (distanceToPlayer > stoppingDistance)
            currentState = EnemyState.Chase;
        else if (distanceToPlayer <= stoppingDistance && distanceToPlayer > retreatDistance)
            currentState = EnemyState.Attack; 
        else if (distanceToPlayer <= retreatDistance)
            currentState = EnemyState.Chase; 

        ExecuteState(distanceToPlayer);
    }

    void ExecuteState(float distance)
    {
        switch (currentState)
        {
            case EnemyState.Chase:
                if (distance > stoppingDistance)
                    transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
                else if (distance < retreatDistance)
                    transform.position = Vector2.MoveTowards(transform.position, player.position, -moveSpeed * Time.deltaTime);
                break;

            case EnemyState.Attack:
                if (Time.time >= nextFireTime)
                {
                    Shoot();
                    nextFireTime = Time.time + fireRate;
                }
                break;
            case EnemyState.Idle:
                break;
        }
    }

    void Shoot()
    {
        if (enemyBulletPrefab != null && firePoint != null && player != null)
        {
            Vector2 direction = (player.position - firePoint.position).normalized;
            GameObject bullet = Instantiate(enemyBulletPrefab, firePoint.position, Quaternion.identity);
            
            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
            if (bulletRb != null)
            {
                bulletRb.AddForce(direction * bulletSpeed, ForceMode2D.Impulse);
            }
        }
    }
}
