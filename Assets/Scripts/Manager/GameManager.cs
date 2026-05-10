// GameManager.cs
using UnityEngine;
using TMPro;

/// <summary>
/// Singleton quản lý trạng thái game: đếm kill, kích hoạt Boss.
/// Các script khác gọi GameManager.Instance để truy cập.
/// </summary>
public class GameManager : MonoBehaviour
{
    // ── Singleton ──────────────────────────────────────────────
    public static GameManager Instance { get; private set; }

    // ── Cài đặt Boss ───────────────────────────────────────────
    [Header("Cài đặt Boss")]
    public GameObject bossPrefab;          // Kéo Boss Prefab vào đây
    public int minKillsToSpawnBoss = 20;  // Ngưỡng tối thiểu
    public int maxKillsToSpawnBoss = 20;  // Ngưỡng tối đa
    public Transform player;               // Kéo Player vào đây

    // ── UI Kill Counter ────────────────────────────────────────
    [Header("UI Kill Counter")]
    public TextMeshProUGUI killCountText;  // Kéo Text "Kills" vào đây

    // ── Biến nội bộ ────────────────────────────────────────────
    private int currentKills = 0;          // Số kill hiện tại
    private int killThreshold;             // Ngưỡng ngẫu nhiên để Boss xuất hiện
    private bool bossSpawned = false;      // Boss đã được triệu hồi chưa?
    private EnemySpawner enemySpawner;     // Tham chiếu để dừng spawn quái khi Boss xuất hiện

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
        // Tìm EnemySpawner trong scene
        enemySpawner = FindAnyObjectByType<EnemySpawner>();

        // Tự động tìm Player nếu quên kéo thả
        if (player == null)
            player = GameObject.FindWithTag("Player")?.transform;

        // Roll ngẫu nhiên ngưỡng Boss ngay từ đầu game
        killThreshold = Random.Range(minKillsToSpawnBoss, maxKillsToSpawnBoss + 1);

        Debug.Log($"[GameManager] Boss sẽ xuất hiện tại: {killThreshold} kills");

        // Cập nhật UI ban đầu
        UpdateKillUI();
    }

    // ──────────────────────────────────────────────────────────
    /// <summary>
    /// Hàm này được EnemyHealth.cs gọi mỗi khi 1 con quái chết.
    /// </summary>
    public void RegisterKill()
    {
        // Nếu Boss đã ra rồi thì không đếm thêm nữa
        if (bossSpawned) return;

        currentKills++;
        UpdateKillUI();

        Debug.Log($"[GameManager] Kill: {currentKills} / {killThreshold}");

        // Kiểm tra ngưỡng Boss
        if (currentKills >= killThreshold)
        {
            SpawnBoss();
        }
    }

    // ──────────────────────────────────────────────────────────
    /// <summary>
    /// Triệu hồi Boss và dừng spawn quái thường.
    /// </summary>
    private void SpawnBoss()
    {
        if (bossSpawned) return;
        bossSpawned = true;

        Debug.Log("[GameManager] *** BOSS XUẤT HIỆN ***");

        // Dừng EnemySpawner để quái thường không spawn nữa
        if (enemySpawner != null)
            enemySpawner.enabled = false;

        // Spawn Boss cách player 10 units về bên phải
        if (bossPrefab != null && player != null)
        {
            Vector3 spawnPos = player.position + new Vector3(10f, 0f, 0f);
            Instantiate(bossPrefab, spawnPos, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("[GameManager] Chưa gán bossPrefab hoặc player!");
        }

        // Cập nhật UI: đổi text thành "BOSS!" 
        if (killCountText != null)
            killCountText.text = "⚠ BOSS!";
    }

    // ──────────────────────────────────────────────────────────
    /// <summary>
    /// Cập nhật text hiển thị số kill trên HUD.
    /// </summary>
    private void UpdateKillUI()
    {
        if (killCountText != null)
            killCountText.text = $"Kills: {currentKills} / {killThreshold}";
    }

    // ── Getter công khai ───────────────────────────────────────
    public int GetKills() => currentKills;
    public bool IsBossSpawned() => bossSpawned;
}