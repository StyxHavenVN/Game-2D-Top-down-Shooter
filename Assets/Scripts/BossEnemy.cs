using UnityEngine;
using System.Collections;

/// <summary>
/// Boss Enemy 3 Phase — Hệ thống Boss hoành tráng với 3 giai đoạn.
/// 
/// CÁCH DÙNG:
///   1. Tạo Prefab Boss mới (hoặc clone từ Boss cũ)
///   2. Gắn script này + EnemyHealth + EnemyFlash + Rigidbody2D + Collider2D
///   3. Gắn EnemyData (với isBoss = true, maxHP = 60+)
///   4. Kéo bossBulletPrefab vào slot (dùng chung với BossController cũ)
///   5. Kéo walkerPrefab vào summonPrefab (để Boss triệu hồi minion)
///   6. (Tùy chọn) Gắn 3 sprites vào phaseSprites[] để đổi ngoại hình
///
/// PHASE BREAKDOWN:
///   Phase 1 (100%-66% HP): Orbit xung quanh Player + bắn đơn
///   Phase 2 (66%-33% HP):  Đuổi theo Player + triệu hồi quái + bắn chùm
///   Phase 3 (33%-0% HP):   Lao tới Player + bắn 360° điên cuồng + rung camera
/// </summary>
public class BossEnemy : MonoBehaviour
{
    // ── Phase ────────────────────────────────────────────────────
    public enum BossPhase { Phase1_Orbit, Phase2_Summon, Phase3_Chaos }

    [Header("Dữ liệu Boss (Kéo Boss_Data vào đây)")]
    public EnemyData enemyData;

    [Header("Trạng thái")]
    public BossPhase currentPhase = BossPhase.Phase1_Orbit;

    // ── Tấn công: Đạn ────────────────────────────────────────────
    [Header("Vũ khí")]
    [Tooltip("Prefab đạn Boss (dùng chung với BossController cũ)")]
    public GameObject bossBulletPrefab;

    [Tooltip("Điểm bắn chính (Empty GameObject con của Boss)")]
    public Transform firePoint;

    [Tooltip("Số đạn khi bắn chùm 360°")]
    public int spreadBulletCount = 12;

    // ── Tấn công: Triệu hồi ──────────────────────────────────────
    [Header("Triệu hồi (Phase 2)")]
    [Tooltip("Prefab quái được triệu hồi (ví dụ: BasicEnemy/Walker)")]
    public GameObject summonPrefab;

    [Tooltip("Số quái triệu hồi mỗi lần")]
    public int summonCount = 3;

    // ── Phase Sprites (Tùy chọn) ─────────────────────────────────
    [Header("Ngoại hình Phase (Tùy chọn)")]
    [Tooltip("3 sprites cho 3 phase — để trống nếu không cần đổi")]
    public Sprite[] phaseSprites;

    // ── Cài đặt Orbit (Phase 1) ──────────────────────────────────
    [Header("Phase 1: Orbit")]
    public float orbitRadius = 5f;
    public float orbitSpeed = 2f;
    public float singleShotInterval = 0.8f;

    // ── Cài đặt Chase (Phase 2) ──────────────────────────────────
    [Header("Phase 2: Chase + Summon")]
    public float chaseSpeed = 3f;
    public float summonInterval = 5f;
    public float spreadShotInterval = 2f;

    // ── Cài đặt Chaos (Phase 3) ──────────────────────────────────
    [Header("Phase 3: Chaos Barrage")]
    public float chaosSpeedMultiplier = 1.5f;
    public float barrageShotInterval = 0.3f;
    public float dashSpeed = 18f;
    public float dashDuration = 0.4f;

    // ── Sát thương va chạm ────────────────────────────────────────
    [Header("Sát thương chạm")]
    public float contactDamage = 10f;
    public float contactCooldown = 0.8f;

    // ── Biến nội bộ ──────────────────────────────────────────────
    private Transform player;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private EnemyHealth enemyHealth;
    private float moveSpeed = 2f;
    private float bulletForce = 8f;
    private float contactAttackTimer = 0f;
    private float orbitAngle = 0f;
    private bool isTransitioning = false;
    private bool isDead = false;
    private Vector3 originalScale;
    private Coroutine currentAIRoutine;

    // ══════════════════════════════════════════════════════════════
    //  KHỞI TẠO
    // ══════════════════════════════════════════════════════════════

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        enemyHealth = GetComponent<EnemyHealth>();
        originalScale = transform.localScale;
    }

    void Start()
    {
        // Đọc thông số từ EnemyData
        if (enemyData != null)
        {
            moveSpeed = enemyData.moveSpeed;
            bulletForce = enemyData.bulletSpeed > 0 ? enemyData.bulletSpeed : 8f;
            contactDamage = enemyData.damage;
        }

        // Tìm Player
        FindPlayer();

        // Bắt đầu AI Phase 1
        currentPhase = BossPhase.Phase1_Orbit;
        currentAIRoutine = StartCoroutine(BossAI());

        Debug.Log("[BossEnemy] ★ BOSS ĐÃ XUẤT HIỆN — Phase 1: Orbit ★");
    }

    private void FindPlayer()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj == null) playerObj = GameObject.Find("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    // ══════════════════════════════════════════════════════════════
    //  UPDATE — Theo dõi HP để chuyển Phase
    // ══════════════════════════════════════════════════════════════

    void Update()
    {
        if (player == null || isDead) return;

        // Giảm cooldown sát thương va chạm
        if (contactAttackTimer > 0f)
            contactAttackTimer -= Time.deltaTime;

        // Lật sprite theo hướng nhìn về Player
        if (spriteRenderer != null)
        {
            float dirX = player.position.x - transform.position.x;
            if (dirX > 0.1f) spriteRenderer.flipX = false;
            else if (dirX < -0.1f) spriteRenderer.flipX = true;
        }

        // ── KIỂM TRA CHUYỂN PHASE DỰA VÀO HP ──
        CheckPhaseTransition();
    }

    /// <summary>
    /// Kiểm tra HP hiện tại để chuyển phase.
    /// Phase 1: 100% - 66% HP
    /// Phase 2: 66% - 33% HP
    /// Phase 3: 33% - 0% HP
    /// </summary>
    private void CheckPhaseTransition()
    {
        if (enemyHealth == null || isTransitioning) return;

        float hpPercent = GetHealthPercent();

        if (currentPhase == BossPhase.Phase1_Orbit && hpPercent <= 0.66f)
        {
            StartCoroutine(TransitionToPhase(BossPhase.Phase2_Summon));
        }
        else if (currentPhase == BossPhase.Phase2_Summon && hpPercent <= 0.33f)
        {
            StartCoroutine(TransitionToPhase(BossPhase.Phase3_Chaos));
        }
    }

    /// <summary>
    /// Lấy tỷ lệ HP hiện tại (0.0 → 1.0)
    /// Đọc trực tiếp từ EnemyHealth (currentHealth là private → dùng healthFill)
    /// </summary>
    private float GetHealthPercent()
    {
        if (enemyHealth == null) return 1f;

        // Dùng healthFill.fillAmount nếu có (đọc gián tiếp từ EnemyHealth)
        if (enemyHealth.healthFill != null)
            return enemyHealth.healthFill.fillAmount;

        // Fallback: ước lượng từ maxHealth
        return 1f;
    }

    // ══════════════════════════════════════════════════════════════
    //  CHUYỂN PHASE
    // ══════════════════════════════════════════════════════════════

    /// <summary>
    /// Hiệu ứng chuyển phase: dừng AI cũ → flash nhấp nháy → đổi sprite → bắt đầu AI mới
    /// </summary>
    private IEnumerator TransitionToPhase(BossPhase newPhase)
    {
        isTransitioning = true;

        // Dừng AI hiện tại
        if (currentAIRoutine != null)
            StopCoroutine(currentAIRoutine);

        rb.linearVelocity = Vector2.zero;

        Debug.Log($"[BossEnemy] ★ CHUYỂN PHASE → {newPhase} ★");

        // ── HIỆU ỨNG CHUYỂN PHASE ──

        // 1. Rung camera mạnh (cảnh báo Player)
        if (CameraShake.Instance != null)
            CameraShake.Instance.Shake(0.3f, 0.15f);

        // 2. Nhấp nháy sprite (invincible cảm giác)
        yield return StartCoroutine(PhaseTransitionFlash());

        // 3. Đổi sprite (nếu có)
        int phaseIndex = (int)newPhase;
        if (phaseSprites != null && phaseIndex < phaseSprites.Length && phaseSprites[phaseIndex] != null)
        {
            spriteRenderer.sprite = phaseSprites[phaseIndex];
        }

        // 4. Scale up mỗi phase (to dần → đáng sợ hơn)
        float scaleMultiplier = 1f + phaseIndex * 0.15f; // Phase2: 1.15x, Phase3: 1.3x
        transform.localScale = originalScale * scaleMultiplier;

        // 5. Cập nhật phase
        currentPhase = newPhase;
        isTransitioning = false;

        // 6. Bắt đầu AI mới
        currentAIRoutine = StartCoroutine(BossAI());
    }

    /// <summary>
    /// Nhấp nháy sprite 1.5 giây khi chuyển phase (hiệu ứng cảnh báo)
    /// </summary>
    private IEnumerator PhaseTransitionFlash()
    {
        if (spriteRenderer == null) yield break;

        Color originalColor = spriteRenderer.color;
        float flashDuration = 1.5f;
        float elapsed = 0f;
        int flashCount = 0;

        while (elapsed < flashDuration)
        {
            // Nhấp nháy giữa đỏ và trắng
            spriteRenderer.color = (flashCount % 2 == 0) ? Color.red : Color.white;
            flashCount++;
            elapsed += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }

        spriteRenderer.color = originalColor;
    }

    // ══════════════════════════════════════════════════════════════
    //  BOSS AI — ĐIỀU PHỐI PHASE
    // ══════════════════════════════════════════════════════════════

    /// <summary>
    /// Vòng lặp AI chính — chạy pattern tùy theo phase hiện tại.
    /// </summary>
    private IEnumerator BossAI()
    {
        while (!isDead)
        {
            if (player == null) { yield return null; continue; }

            switch (currentPhase)
            {
                case BossPhase.Phase1_Orbit:
                    yield return StartCoroutine(Phase1_OrbitAndShoot());
                    break;

                case BossPhase.Phase2_Summon:
                    yield return StartCoroutine(Phase2_SummonAndChase());
                    break;

                case BossPhase.Phase3_Chaos:
                    yield return StartCoroutine(Phase3_ChaosBarrage());
                    break;
            }

            yield return null; // Tránh infinite loop nếu phase kết thúc ngay
        }
    }

    // ══════════════════════════════════════════════════════════════
    //  PHASE 1: ORBIT + SINGLE SHOT
    //  Boss bay vòng quanh Player, bắn đạn đơn nhắm thẳng vào Player
    // ══════════════════════════════════════════════════════════════

    private IEnumerator Phase1_OrbitAndShoot()
    {
        float shotTimer = 0f;

        while (currentPhase == BossPhase.Phase1_Orbit && !isDead)
        {
            if (player == null) { yield return null; continue; }

            // Di chuyển theo quỹ đạo tròn quanh Player
            orbitAngle += orbitSpeed * Time.deltaTime;
            Vector2 orbitTarget = (Vector2)player.position + new Vector2(
                Mathf.Cos(orbitAngle) * orbitRadius,
                Mathf.Sin(orbitAngle) * orbitRadius
            );

            // Di chuyển mượt tới vị trí quỹ đạo
            rb.MovePosition(Vector2.MoveTowards(rb.position, orbitTarget, moveSpeed * 2f * Time.fixedDeltaTime));

            // Bắn đạn đơn theo interval
            shotTimer += Time.deltaTime;
            if (shotTimer >= singleShotInterval)
            {
                ShootAtPlayer();
                shotTimer = 0f;
            }

            yield return null;
        }
    }

    // ══════════════════════════════════════════════════════════════
    //  PHASE 2: SUMMON MINIONS + CHASE + SPREAD SHOT
    //  Boss đuổi theo Player, triệu hồi quái, bắn chùm 3 viên
    // ══════════════════════════════════════════════════════════════

    private IEnumerator Phase2_SummonAndChase()
    {
        float summonTimer = 0f;
        float spreadTimer = 0f;

        // Triệu hồi ngay 1 đợt khi vào Phase 2
        SummonMinions();

        while (currentPhase == BossPhase.Phase2_Summon && !isDead)
        {
            if (player == null) { yield return null; continue; }

            // Đuổi theo Player
            Vector2 direction = ((Vector2)player.position - rb.position).normalized;
            rb.MovePosition(rb.position + direction * chaseSpeed * Time.fixedDeltaTime);

            // Triệu hồi quái định kỳ
            summonTimer += Time.deltaTime;
            if (summonTimer >= summonInterval)
            {
                SummonMinions();
                summonTimer = 0f;
            }

            // Bắn chùm 3 viên định kỳ
            spreadTimer += Time.deltaTime;
            if (spreadTimer >= spreadShotInterval)
            {
                ShootSpread(5);

                // Rung camera nhẹ mỗi lần bắn chùm
                if (CameraShake.Instance != null)
                    CameraShake.Instance.Shake(0.08f, 0.05f);

                spreadTimer = 0f;
            }

            yield return null;
        }
    }

    // ══════════════════════════════════════════════════════════════
    //  PHASE 3: CHAOS BARRAGE + DASH + 360° BULLETS
    //  Boss điên cuồng: lao tới Player, bắn 360° tất cả hướng
    // ══════════════════════════════════════════════════════════════

    private IEnumerator Phase3_ChaosBarrage()
    {
        float barrageTimer = 0f;
        float dashCooldown = 3f;
        float dashTimer = 0f;

        while (currentPhase == BossPhase.Phase3_Chaos && !isDead)
        {
            if (player == null) { yield return null; continue; }

            // Đuổi nhanh hơn Phase 2
            Vector2 direction = ((Vector2)player.position - rb.position).normalized;
            float currentSpeed = chaseSpeed * chaosSpeedMultiplier;
            rb.MovePosition(rb.position + direction * currentSpeed * Time.fixedDeltaTime);

            // Bắn 360° liên tục (barrage)
            barrageTimer += Time.deltaTime;
            if (barrageTimer >= barrageShotInterval)
            {
                Shoot360();
                barrageTimer = 0f;
            }

            // Dash xuyên qua Player định kỳ
            dashTimer += Time.deltaTime;
            if (dashTimer >= dashCooldown)
            {
                yield return StartCoroutine(DashAttack());
                dashTimer = 0f;
            }

            yield return null;
        }
    }

    // ══════════════════════════════════════════════════════════════
    //  CÁC KỸ NĂNG TẤN CÔNG
    // ══════════════════════════════════════════════════════════════

    /// <summary>
    /// Bắn 1 viên đạn thẳng vào Player.
    /// </summary>
    private void ShootAtPlayer()
    {
        if (bossBulletPrefab == null || player == null) return;

        Vector2 dir = ((Vector2)player.position - (Vector2)transform.position).normalized;
        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;

        GameObject bullet = Instantiate(bossBulletPrefab, spawnPos, Quaternion.identity);
        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
        if (bulletRb != null)
            bulletRb.AddForce(dir * bulletForce, ForceMode2D.Impulse);

        // Tự hủy đạn sau 5 giây
        Destroy(bullet, 5f);
    }

    /// <summary>
    /// Bắn chùm N viên đạn theo hình quạt nhắm vào Player.
    /// </summary>
    private void ShootSpread(int count)
    {
        if (bossBulletPrefab == null || player == null) return;

        Vector2 baseDir = ((Vector2)player.position - (Vector2)transform.position).normalized;
        float spreadAngle = 15f; // Góc giữa mỗi viên đạn (độ)

        for (int i = 0; i < count; i++)
        {
            float angleOffset = (i - count / 2f + 0.5f) * spreadAngle;
            Vector2 dir = RotateVector(baseDir, angleOffset);

            Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
            GameObject bullet = Instantiate(bossBulletPrefab, spawnPos, Quaternion.identity);

            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
            if (bulletRb != null)
                bulletRb.AddForce(dir * bulletForce, ForceMode2D.Impulse);

            Destroy(bullet, 5f);
        }
    }

    /// <summary>
    /// Bắn 360° — đạn bay đều ra mọi hướng (Phase 3).
    /// </summary>
    private void Shoot360()
    {
        if (bossBulletPrefab == null) return;

        float angleStep = 360f / spreadBulletCount;
        float startAngle = Random.Range(0f, angleStep); // Xoay ngẫu nhiên mỗi lần bắn

        for (int i = 0; i < spreadBulletCount; i++)
        {
            float angle = startAngle + angleStep * i;
            float rad = angle * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

            Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
            GameObject bullet = Instantiate(bossBulletPrefab, spawnPos, Quaternion.identity);

            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
            if (bulletRb != null)
                bulletRb.AddForce(dir * bulletForce * 0.8f, ForceMode2D.Impulse);

            Destroy(bullet, 5f);
        }

        // Rung camera mỗi lần bắn 360°
        if (CameraShake.Instance != null)
            CameraShake.Instance.Shake(0.12f, 0.08f);
    }

    /// <summary>
    /// Lao nhanh về phía Player (Dash Attack).
    /// </summary>
    private IEnumerator DashAttack()
    {
        if (player == null) yield break;

        // Chuẩn bị dash: đổi màu đỏ cảnh báo
        if (spriteRenderer != null)
            spriteRenderer.color = Color.red;

        // Chờ 0.3s để Player phản ứng
        yield return new WaitForSeconds(0.3f);

        // LAO TỚI!
        Vector2 dashDirection = ((Vector2)player.position - rb.position).normalized;
        float startTime = Time.time;

        while (Time.time < startTime + dashDuration)
        {
            rb.linearVelocity = dashDirection * dashSpeed;
            yield return null;
        }

        rb.linearVelocity = Vector2.zero;

        // Rung camera cực mạnh
        if (CameraShake.Instance != null)
            CameraShake.Instance.Shake(0.25f, 0.12f);

        // Bắn 360° ngay sau khi dash
        Shoot360();

        // Reset màu
        if (spriteRenderer != null)
            spriteRenderer.color = Color.white;

        yield return new WaitForSeconds(0.5f);
    }

    /// <summary>
    /// Triệu hồi quái nhỏ xung quanh Boss.
    /// </summary>
    private void SummonMinions()
    {
        if (summonPrefab == null) return;

        Debug.Log($"[BossEnemy] Triệu hồi {summonCount} quái!");

        for (int i = 0; i < summonCount; i++)
        {
            Vector2 spawnPos = (Vector2)transform.position + Random.insideUnitCircle * 3f;

            // Dùng ObjectPool nếu có
            GameObject minion;
            if (ObjectPool.Instance != null)
                minion = ObjectPool.Instance.GetEnemy(summonPrefab, spawnPos);
            else
                minion = Instantiate(summonPrefab, spawnPos, Quaternion.identity);

            // Reset máu quái triệu hồi
            if (minion != null)
            {
                EnemyHealth minionHealth = minion.GetComponent<EnemyHealth>();
                if (minionHealth != null)
                    minionHealth.ResetHealth();
            }
        }

        // Hiệu ứng triệu hồi: rung camera nhẹ
        if (CameraShake.Instance != null)
            CameraShake.Instance.Shake(0.15f, 0.1f);
    }

    // ══════════════════════════════════════════════════════════════
    //  SÁT THƯƠNG VA CHẠM
    // ══════════════════════════════════════════════════════════════

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.name == "Player")
        {
            if (contactAttackTimer <= 0f)
            {
                Health playerHealth = collision.gameObject.GetComponent<Health>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(Mathf.RoundToInt(contactDamage));
                }
                contactAttackTimer = contactCooldown;
            }
        }
    }

    // ══════════════════════════════════════════════════════════════
    //  SỰ KIỆN CHẾT — THÔNG BÁO GAME MANAGER
    // ══════════════════════════════════════════════════════════════

    /// <summary>
    /// Khi Boss bị tắt (chết qua EnemyHealth.Die() → ReturnEnemy/SetActive(false)),
    /// thông báo GameManager để kích hoạt Victory.
    /// </summary>
    void OnDisable()
    {
        // Chỉ gọi Victory nếu Boss thực sự đã chết (isDead từ EnemyHealth)
        // và game đang chạy (tránh gọi khi thoát game)
        if (isDead) return;
        isDead = true;

        // Dừng tất cả Coroutine
        StopAllCoroutines();

        // Rung camera CỰC MẠNH khi Boss chết
        if (CameraShake.Instance != null)
            CameraShake.Instance.Shake(0.5f, 0.3f);

        // Thông báo GameManager: Boss đã bị tiêu diệt!
        if (GameManager.Instance != null)
            GameManager.Instance.BossDefeated();

        Debug.Log("[BossEnemy] ★★★ BOSS ĐÃ BỊ TIÊU DIỆT! ★★★");
    }

    // ══════════════════════════════════════════════════════════════
    //  UTILITIES
    // ══════════════════════════════════════════════════════════════

    /// <summary>
    /// Xoay vector 2D theo góc (độ).
    /// </summary>
    private Vector2 RotateVector(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        return new Vector2(
            v.x * Mathf.Cos(rad) - v.y * Mathf.Sin(rad),
            v.x * Mathf.Sin(rad) + v.y * Mathf.Cos(rad)
        );
    }

    /// <summary>
    /// Vẽ debug gizmo cho bán kính orbit và tầm phát hiện.
    /// </summary>
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f); // Cam
        Gizmos.DrawWireSphere(transform.position, orbitRadius);

        Gizmos.color = new Color(1f, 0f, 0f, 0.2f); // Đỏ
        Gizmos.DrawWireSphere(transform.position, 2f); // Contact range
    }
}
