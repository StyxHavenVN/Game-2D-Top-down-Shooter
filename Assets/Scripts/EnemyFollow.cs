using UnityEngine;

public class EnemyFollow : MonoBehaviour
{
    public Transform player; // Biến để chứa vị trí của người chơi
    public float moveSpeed = 3f; // Tốc độ của quái (nên chậm hơn Player một chút để bạn còn chạy trốn được)

    private Rigidbody2D rb;
    private Vector2 movement;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Dòng code tiện lợi: Tự động tìm nhân vật tên "Player" trên màn hình nếu bạn quên kéo thả
        if (player == null)
        {
            player = GameObject.Find("Player").transform;
        }
    }

    void Update()
    {
        if (player != null)
        {
            // Toán học cơ bản: Hướng đi = Vị trí đích (Player) - Vị trí hiện tại (Enemy)
            Vector3 direction = player.position - transform.position;

            // Chuẩn hóa vector để tốc độ luôn đều đặn, không bị giật cục
            direction.Normalize();
            movement = direction;
        }
    }

    void FixedUpdate()
    {
        // Di chuyển quái vật về phía người chơi
        rb.MovePosition((Vector2)transform.position + (movement * moveSpeed * Time.fixedDeltaTime));
    }
    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.name == "Player")
        {
            Health playerHealth = collision.gameObject.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(1); // Mỗi khung hình chạm vào mất 1 máu
            }
        }
    }
}