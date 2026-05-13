using UnityEngine;
using System.Collections;

/// <summary>
/// Quái Kamikaze (ôm bom tự sát).
/// Hành vi: Lao cực nhanh về phía Player. Khi đến gần → phình to → NỔ TUNG.
/// Gây sát thương vùng (AoE) cho Player nếu đứng trong bán kính nổ.
/// 
/// Cách dùng:
/// 1. Tạo Prefab quái mới (ví dụ: hình tròn đỏ, nhỏ hơn quái thường).
/// 2. Gắn script này + EnemyHealth + EnemyFlash + Rigidbody2D + Collider2D.
/// 3. Gắn Tag "Enemy" cho nó.
/// 4. Tạo file EnemyData cho Kamikaze (moveSpeed cao, HP thấp).
/// </summary>
public class KamikazeEnemy : MonoBehaviour
{
    [Header("Dữ liệu Quái (Kéo EnemyData vào)")]
    public EnemyData enemyData;

    [Header("Cài đặt Nổ")]
    [Tooltip("Bán kính gây sát thương khi nổ")]
    public float explosionRadius = 2.5f;

    [Tooltip("Sát thương gây ra khi nổ")]
    public float explosionDamage = 25f;

    [Tooltip("Khoảng cách kích hoạt tự nổ (đến gần Player bao nhiêu thì nổ)")]
    public float detonateDistance = 1.2f;

    [Tooltip("Thời gian phình to trước khi nổ (tạo cảm giác cảnh báo cho Player)")]
    public float fuseTime = 0.5f;

    [Header("Hiệu ứng (Tùy chọn)")]
    [Tooltip("Prefab hạt nổ (Particle System)")]
    public GameObject explosionVFXPrefab;

    // Biến nội bộ
    private Transform player;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private float moveSpeed = 5f;
    private float damage = 10f;
    private bool isDetonating = false;
    private Vector3 originalScale;
    private bool hasInitialized = false;

    void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;
        hasInitialized = true;

        // Đọc thông số từ thẻ Data
        if (enemyData != null)
        {
            moveSpeed = enemyData.moveSpeed;
            damage = enemyData.damage;
            explosionDamage = damage * 2.5f; // Sát thương nổ = 2.5x sát thương thường
        }

        FindPlayer();
    }

    void OnEnable()
    {
        // Reset trạng thái khi lấy ra từ Pool
        isDetonating = false;
        if (hasInitialized)
        {
            transform.localScale = originalScale;
            // Reset màu sprite
            if (spriteRenderer != null)
                spriteRenderer.color = Color.white;
        }
        FindPlayer();
    }

    private void FindPlayer()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj == null) playerObj = GameObject.Find("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }
    }

    void Update()
    {
        if (player == null || isDetonating) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Lật sprite theo hướng di chuyển
        if (spriteRenderer != null)
        {
            float dirX = player.position.x - transform.position.x;
            if (dirX > 0.1f) spriteRenderer.flipX = false;
            else if (dirX < -0.1f) spriteRenderer.flipX = true;
        }

        if (distanceToPlayer <= detonateDistance)
        {
            // Đến gần rồi — BẮT ĐẦU QUÁ TRÌNH TỰ HỦY!
            StartCoroutine(DetonateRoutine());
        }
    }

    void FixedUpdate()
    {
        if (player == null || isDetonating) return;

        // Lao thẳng về phía Player với tốc độ cao
        Vector2 direction = (player.position - transform.position).normalized;
        rb.MovePosition((Vector2)transform.position + direction * moveSpeed * Time.fixedDeltaTime);
    }

    /// <summary>
    /// Quy trình tự hủy: Phình to → Nổ → Gây sát thương vùng → Chết.
    /// </summary>
    private IEnumerator DetonateRoutine()
    {
        isDetonating = true;
        rb.linearVelocity = Vector2.zero; // Dừng lại

        // ---- GIAI ĐOẠN 1: PHÌNH TO (cảnh báo cho Player) ----
        float elapsed = 0f;
        Vector3 startScale = transform.localScale;
        Vector3 targetScale = startScale * 1.8f; // Phình to gấp 1.8 lần

        // Đổi màu đỏ rực để cảnh báo
        Color originalColor = Color.white;
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
            spriteRenderer.color = Color.red;
        }

        while (elapsed < fuseTime)
        {
            elapsed += Time.deltaTime;
            // Phình to dần dần
            transform.localScale = Vector3.Lerp(startScale, targetScale, elapsed / fuseTime);
            // Nhấp nháy giữa đỏ và trắng (cảnh báo nguy hiểm!)
            if (spriteRenderer != null)
                spriteRenderer.color = Mathf.PingPong(Time.time * 10f, 1f) > 0.5f ? Color.red : Color.white;
            yield return null;
        }

        // ---- GIAI ĐOẠN 2: NỔ TUNG ----
        Explode();
    }

    private void Explode()
    {
        // 📸 Rung camera CỰC MẠNH khi nổ
        if (CameraShake.Instance != null)
            CameraShake.Instance.Shake(0.35f, 0.2f);

        // 💥 Tạo hiệu ứng hạt nổ (nếu có)
        if (explosionVFXPrefab != null)
        {
            GameObject vfx = Instantiate(explosionVFXPrefab, transform.position, Quaternion.identity);
            Destroy(vfx, 2f); // Tự xóa sau 2s
        }

        // 🔥 Gây sát thương vùng (AoE) — Tìm tất cả đối tượng trong bán kính
        Collider2D[] hitTargets = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (Collider2D target in hitTargets)
        {
            // Gây sát thương cho Player
            if (target.CompareTag("Player"))
            {
                Health playerHealth = target.GetComponent<Health>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(Mathf.RoundToInt(explosionDamage));
                    Debug.Log($"[Kamikaze] NỔ trúng Player! Sát thương: {explosionDamage}");
                }
            }
        }

        // Phát âm thanh nổ (dùng chung tiếng quái chết)
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayEnemyDeath();

        // Tự chết (thông qua EnemyHealth để đếm Kill + rớt đồ)
        EnemyHealth myHealth = GetComponent<EnemyHealth>();
        if (myHealth != null)
        {
            // Gọi TakeDamage với sát thương cực lớn để kích hoạt Die()
            myHealth.TakeDamage(99999f);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    // Vẽ bán kính nổ trong Scene View (để debug)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detonateDistance);
    }
}
