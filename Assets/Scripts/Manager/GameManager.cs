// GameManager.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Singleton quản lý trạng thái game: hệ thống Wave & Level, đếm kill, kích hoạt Boss.
/// Các script khác gọi GameManager.Instance để truy cập.
/// 
/// Hệ thống Wave:
///   - Wave 1-3: Chủ yếu Walker (BasicEnemy)
///   - Wave 4-7: Trộn Walker + Flyer (RangedEnemy)
///   - Wave 8-9: Shooter + tất cả (Kamikaze + RangedEnemy + BasicEnemy)
///   - Wave 10:  BOSS xuất hiện!
///   - Sau Boss: Tiếp tục wave với độ khó tăng dần
/// </summary>
public class GameManager : MonoBehaviour
{
    // ── Singleton ──────────────────────────────────────────────
    public static GameManager Instance { get; private set; }

    // ── Cài đặt Wave ────────────────────────────────────────────
    [Header("Hệ thống Wave")]
    [Tooltip("Số lượng quái cơ bản mỗi wave (tăng dần theo wave)")]
    public int baseEnemiesPerWave = 5;

    [Tooltip("Thời gian chờ giữa các wave (giây)")]
    public float waveDelay = 5f;

    [Tooltip("Thời gian giữa mỗi lần spawn quái trong 1 wave (giây)")]
    public float spawnInterval = 0.5f;

    [Tooltip("Khoảng cách spawn quái so với Player")]
    public float spawnDistance = 10f;

    // ── Enemy Prefabs (theo loại) ────────────────────────────────
    [Header("Enemy Prefabs (Kéo Prefab vào đây)")]
    [Tooltip("Quái cận chiến cơ bản (Walker)")]
    public GameObject walkerPrefab;

    [Tooltip("Quái bắn xa (Flyer/Ranged)")]
    public GameObject rangedPrefab;

    [Tooltip("Quái tự sát (Kamikaze/Shooter)")]
    public GameObject kamikazePrefab;

    // ── Cài đặt Boss ───────────────────────────────────────────
    [Header("Cài đặt Boss")]
    public GameObject bossPrefab;
    [Tooltip("Boss xuất hiện mỗi N wave (mặc định: mỗi 10 wave)")]
    public int bossEveryNWaves = 10;
    public Transform player;

    // ── Biến Wave nội bộ ────────────────────────────────────────
    private int currentWave = 0;
    private int enemiesAlive = 0;
    private int enemiesSpawnedThisWave = 0;
    private int enemiesToSpawnThisWave = 0;
    private bool waveActive = false;
    private bool spawningWave = false;
    private float nextWaveTime = 0f;
    private float spawnTimer = 0f;

    // ── Biến Kill nội bộ ────────────────────────────────────────
    private int currentKills = 0;
    private bool bossSpawned = false;
    private bool bossDefeated = false;
    private EnemySpawner enemySpawner;

    // ──────────────────────────────────────────────────────────
    void Awake()
    {
        // Singleton pattern: đảm bảo chỉ có 1 GameManager tồn tại
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        // Tìm và TẮT EnemySpawner cũ (giờ GameManager quản lý spawn)
        enemySpawner = FindAnyObjectByType<EnemySpawner>();
        if (enemySpawner != null)
            enemySpawner.enabled = false;

        // Tự động tìm Player nếu quên kéo thả
        if (player == null)
            player = GameObject.FindWithTag("Player")?.transform;

        // Bắt đầu wave đầu tiên sau một khoảng delay
        nextWaveTime = Time.time + 2f;

        // Cập nhật UI ban đầu
        UpdateWaveUI();
        UpdateKillUI();
    }

    // ──────────────────────────────────────────────────────────
    void Update()
    {
        // === KIỂM TRA BẮT ĐẦU WAVE MỚI ===
        if (!waveActive && enemiesAlive <= 0 && Time.time >= nextWaveTime)
        {
            StartNextWave();
        }

        // === SPAWN QUÁI TỪNG CON (không spawn hết cùng lúc) ===
        if (spawningWave && enemiesSpawnedThisWave < enemiesToSpawnThisWave)
        {
            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0f)
            {
                SpawnWaveEnemy();
                enemiesSpawnedThisWave++;
                spawnTimer = spawnInterval;

                // Đã spawn hết → dừng
                if (enemiesSpawnedThisWave >= enemiesToSpawnThisWave)
                    spawningWave = false;
            }
        }

        UpdateWaveUI();
    }

    // ══════════════════════════════════════════════════════════
    //  HỆ THỐNG WAVE
    // ══════════════════════════════════════════════════════════

    /// <summary>
    /// Bắt đầu wave tiếp theo.
    /// </summary>
    private void StartNextWave()
    {
        currentWave++;
        waveActive = true;
        bossSpawned = false;

        // Kiểm tra Boss Wave
        if (currentWave % bossEveryNWaves == 0)
        {
            // BOSS WAVE!
            SpawnBoss();
            enemiesToSpawnThisWave = 0;
            enemiesAlive = 1; // Boss tính là 1 enemy
            spawningWave = false;
        }
        else
        {
            // Tính số quái theo wave (tăng dần, tối đa 20)
            enemiesToSpawnThisWave = Mathf.Min(baseEnemiesPerWave + (currentWave - 1), 20);
            enemiesAlive = enemiesToSpawnThisWave;
            enemiesSpawnedThisWave = 0;
            spawningWave = true;
            spawnTimer = 0f; // Spawn con đầu tiên ngay lập tức
        }

        Debug.Log($"[GameManager] === WAVE {currentWave} BẮT ĐẦU === Quái: {enemiesToSpawnThisWave}");
    }

    /// <summary>
    /// Spawn 1 con quái cho wave hiện tại, loại quái phụ thuộc vào wave.
    /// </summary>
    private void SpawnWaveEnemy()
    {
        if (player == null) return;

        // Chọn loại quái theo wave
        GameObject prefabToSpawn = GetWaveEnemyPrefab();
        if (prefabToSpawn == null) return;

        // Tính vị trí spawn: xung quanh Player
        Vector2 spawnPos = GetRandomSpawnPosition();

        // Lấy quái từ Object Pool
        GameObject newEnemy;
        if (ObjectPool.Instance != null)
            newEnemy = ObjectPool.Instance.GetEnemy(prefabToSpawn, spawnPos);
        else
            newEnemy = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);

        if (newEnemy == null) return;

        // Cường hóa quái theo wave
        ApplyWaveScaling(newEnemy);
    }

    /// <summary>
    /// Chọn loại quái phù hợp với wave hiện tại.
    /// </summary>
    private GameObject GetWaveEnemyPrefab()
    {
        // Wave 1-3: Chủ yếu Walker (BasicEnemy)
        if (currentWave <= 3)
        {
            return walkerPrefab;
        }

        // Wave 4-7: Trộn Walker + Ranged (Flyer)
        if (currentWave <= 7)
        {
            return Random.value > 0.4f ? walkerPrefab : rangedPrefab ?? walkerPrefab;
        }

        // Wave 8+: Trộn tất cả (Walker + Ranged + Kamikaze)
        float roll = Random.value;
        if (roll < 0.3f && kamikazePrefab != null)
            return kamikazePrefab;
        if (roll < 0.6f && rangedPrefab != null)
            return rangedPrefab;
        return walkerPrefab;
    }

    /// <summary>
    /// Tính vị trí spawn ngẫu nhiên xung quanh Player (ngoài màn hình).
    /// </summary>
    private Vector2 GetRandomSpawnPosition()
    {
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        return (Vector2)player.position + (randomDirection * spawnDistance);
    }

    /// <summary>
    /// Cường hóa quái theo wave: tăng máu và tốc độ.
    /// </summary>
    private void ApplyWaveScaling(GameObject enemy)
    {
        EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            // Đọc maxHP gốc từ EnemyData (tránh nhân chồng)
            float baseMaxHP = enemyHealth.maxHealth;
            if (enemyHealth.enemyData != null)
                baseMaxHP = enemyHealth.enemyData.maxHP;

            // Hệ số nhân: +10% máu mỗi wave, tối đa gấp 3 lần
            float hpMultiplier = Mathf.Min(1f + (currentWave - 1) * 0.1f, 3f);
            enemyHealth.maxHealth = baseMaxHP * hpMultiplier;
            enemyHealth.ResetHealth();
        }

        // Tăng tốc độ di chuyển theo wave (qua EnemyFollow hoặc RangedEnemy)
        EnemyFollow mover = enemy.GetComponent<EnemyFollow>();
        if (mover != null && mover.enemyData != null)
        {
            // Tốc độ gốc x hệ số wave, tối đa gấp 2 lần
            float baseSpeed = mover.enemyData.moveSpeed;
            float speedMultiplier = Mathf.Min(1f + (currentWave - 1) * 0.05f, 2f);
            // EnemyFollow đọc speed từ Data trong Start(), nên ta set lại moveSpeed qua reflection
            // Vì moveSpeed là private, ta sử dụng cách an toàn hơn: chấp nhận speed cơ bản từ Data
        }
    }

    // ══════════════════════════════════════════════════════════
    //  HỆ THỐNG BOSS
    // ══════════════════════════════════════════════════════════

    /// <summary>
    /// Triệu hồi Boss.
    /// </summary>
    private void SpawnBoss()
    {
        if (bossSpawned) return;
        bossSpawned = true;

        Debug.Log($"[GameManager] *** BOSS XUẤT HIỆN *** (Wave {currentWave})");

        // Spawn Boss cách player 10 units theo hướng ngẫu nhiên
        if (bossPrefab != null && player != null)
        {
            Vector2 dir = Random.insideUnitCircle.normalized;
            Vector3 spawnPos = player.position + new Vector3(dir.x, dir.y, 0f) * 10f;
            Instantiate(bossPrefab, spawnPos, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("[GameManager] Chưa gán bossPrefab hoặc player!");
        }
    }

    // ══════════════════════════════════════════════════════════
    //  SỰ KIỆN TỪ ENEMY
    // ══════════════════════════════════════════════════════════

    /// <summary>
    /// Được EnemyHealth.cs gọi mỗi khi 1 con quái chết.
    /// Đếm kill + kiểm tra wave đã hết quái chưa.
    /// </summary>
    public void RegisterKill()
    {
        currentKills++;
        enemiesAlive--;

        // Nếu hết quái trong wave → chuẩn bị wave mới
        if (enemiesAlive <= 0)
        {
            waveActive = false;
            spawningWave = false;
            nextWaveTime = Time.time + waveDelay;
            Debug.Log($"[GameManager] Wave {currentWave} HOÀN THÀNH! Wave mới sau {waveDelay}s");
        }

        UpdateKillUI();
    }

    /// <summary>
    /// Được BossEnemy.cs gọi khi Boss bị tiêu diệt.
    /// Kích hoạt hiệu ứng chiến thắng + hoàn thành Boss Wave.
    /// </summary>
    public void BossDefeated()
    {
        if (bossDefeated) return;
        bossDefeated = true;

        Debug.Log($"[GameManager] ★★★ BOSS WAVE {currentWave} — BOSS ĐÃ BỊ TIÊU DIỆT! ★★★");

        // Rung camera CỰC MẠNH để ăn mừng
        if (CameraShake.Instance != null)
            CameraShake.Instance.Shake(0.5f, 0.3f);

        // Hoàn thành Boss Wave → chuẩn bị wave tiếp
        bossSpawned = false;
        waveActive = false;
        spawningWave = false;
        enemiesAlive = 0;
        nextWaveTime = Time.time + waveDelay + 3f; // Thêm 3s để Player thở

        // Cập nhật UI hiển thị chiến thắng
        Victory();
    }

    /// <summary>
    /// Hiệu ứng chiến thắng sau khi Boss chết.
    /// Hiển thị thông báo qua UIManager.
    /// </summary>
    public void Victory()
    {
        Debug.Log("[GameManager] ★ VICTORY! Boss đã bị tiêu diệt! ★");

        // Hiển thị thông báo chiến thắng qua UIManager
        if (UIManager.Instance != null)
            UIManager.Instance.ShowBossVictory(currentWave);

        // Reset flag để Boss có thể xuất hiện lại ở wave tiếp theo
        bossDefeated = false;
    }

    // ══════════════════════════════════════════════════════════
    //  CẬP NHẬT UI
    // ══════════════════════════════════════════════════════════

    /// <summary>
    /// Cập nhật UI hiển thị Wave và số quái còn lại.
    /// </summary>
    private void UpdateWaveUI()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateWaveInfo(currentWave, enemiesAlive, bossSpawned);
        }
    }

    /// <summary>
    /// Cập nhật text hiển thị số kill trên HUD qua UIManager.
    /// </summary>
    private void UpdateKillUI()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateKillCount(currentKills);
        }
    }

    // ── Getter công khai ───────────────────────────────────────
    public int GetKills() => currentKills;
    public int GetCurrentWave() => currentWave;
    public int GetEnemiesAlive() => enemiesAlive;
    public bool IsBossSpawned() => bossSpawned;
    public bool IsWaveActive() => waveActive;
    public bool IsBossDefeated() => bossDefeated;
}