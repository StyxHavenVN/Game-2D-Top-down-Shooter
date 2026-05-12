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

    [Header("Vũ khí")]
    public GameObject enemyBulletPrefab;
    public Transform firePoint;

    private Transform player;
    private float nextFireTime;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Đọc thông số từ thẻ Data
        if (enemyData != null)
        {
            moveSpeed = enemyData.moveSpeed;
            stoppingDistance = enemyData.preferredDistance; // Tầm đứng bắn
            retreatDistance = stoppingDistance - 2f;        // Lùi lại nếu Player tiến tới
            fireRate = enemyData.attackCooldown;            // Tốc độ bắn
            bulletSpeed = enemyData.bulletSpeed;            // Tốc độ đạn
        }

        GameObject playerObj = GameObject.Find("Player");
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
        if (enemyBulletPrefab != null && firePoint != null)
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
