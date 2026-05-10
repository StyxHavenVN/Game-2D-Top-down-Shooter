using UnityEngine;

public class EnemyFollow : MonoBehaviour
{
    [Header("Dữ liệu Quái Cận Chiến (Kéo EnemyData vào)")]
    public EnemyData enemyData;

    public Transform player;

    private Rigidbody2D rb;
    private Vector2 movement;
    
    // Chỉ số nội tại (đọc từ Data)
    private float moveSpeed = 3f;
    private float damage = 1f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Đọc thông số từ thẻ Data
        if (enemyData != null)
        {
            moveSpeed = enemyData.moveSpeed;
            damage = enemyData.damage;
        }

        if (player == null)
        {
            GameObject pObj = GameObject.Find("Player");
            if (pObj != null) player = pObj.transform;
        }
    }

    void Update()
    {
        if (player != null)
        {
            Vector3 direction = player.position - transform.position;
            direction.Normalize();
            movement = direction;
        }
    }

    void FixedUpdate()
    {
        rb.MovePosition((Vector2)transform.position + (movement * moveSpeed * Time.fixedDeltaTime));
    }
    
    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.name == "Player")
        {
            Health playerHealth = collision.gameObject.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(Mathf.RoundToInt(damage)); // Gây sát thương dựa trên thông số Data
            }
        }
    }
}