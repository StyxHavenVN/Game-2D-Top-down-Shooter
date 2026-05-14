using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Cài đặt Boss")]
    public GameObject bossPrefab;
    public int minKillsToSpawnBoss = 100;
    public int maxKillsToSpawnBoss = 200;
    public Transform player;

    [Header("UI Kill Counter")]
    public TextMeshProUGUI killCountText;

    [Header("Chống cộng kill lúc mới vào game")]
    public float ignoreKillAtStartTime = 0.5f;

    private int currentKills;
    private int killThreshold;
    private bool bossSpawned;
    private EnemySpawner enemySpawner;
    private float gameStartTime;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Reset chắc chắn ngay từ Awake
        currentKills = 0;
        bossSpawned = false;
        gameStartTime = Time.time;
    }

    void Start()
    {
        enemySpawner = FindAnyObjectByType<EnemySpawner>();

        if (player == null)
        {
            GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");

            if (foundPlayer != null)
            {
                player = foundPlayer.transform;
            }
            else
            {
                Debug.LogWarning("[GameManager] Không tìm thấy Player.");
            }
        }

        if (minKillsToSpawnBoss > maxKillsToSpawnBoss)
        {
            int temp = minKillsToSpawnBoss;
            minKillsToSpawnBoss = maxKillsToSpawnBoss;
            maxKillsToSpawnBoss = temp;
        }

        killThreshold = Random.Range(minKillsToSpawnBoss, maxKillsToSpawnBoss + 1);

        currentKills = 0;
        bossSpawned = false;

        Debug.Log("[GameManager] Boss sẽ xuất hiện tại: " + killThreshold + " kills");

        UpdateKillUI();
    }

    public void RegisterKill()
    {
        // Chặn trường hợp enemy bị destroy/chết ngay lúc scene vừa load
        if (Time.time - gameStartTime < ignoreKillAtStartTime)
        {
            Debug.LogWarning("[GameManager] Bỏ qua kill lúc mới vào game.");
            return;
        }

        if (bossSpawned) return;

        currentKills++;
        UpdateKillUI();

        Debug.Log("[GameManager] Kill: " + currentKills + " / " + killThreshold);

        if (currentKills >= killThreshold)
        {
            SpawnBoss();
        }
    }

    private void SpawnBoss()
    {
        if (bossSpawned) return;

        bossSpawned = true;

        Debug.Log("[GameManager] *** BOSS XUẤT HIỆN ***");

        if (enemySpawner != null)
        {
            enemySpawner.enabled = false;
        }

        if (bossPrefab != null && player != null)
        {
            Vector3 spawnPos = player.position + new Vector3(10f, 0f, 0f);
            Instantiate(bossPrefab, spawnPos, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("[GameManager] Chưa gán bossPrefab hoặc player.");
        }

        if (killCountText != null)
        {
            killCountText.text = "⚠ BOSS!";
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBossMusic();
        }
    }

    private void UpdateKillUI()
    {
        if (killCountText != null)
        {
            killCountText.text = "Kills: " + currentKills + " / " + killThreshold;
        }
    }

    public int GetKills()
    {
        return currentKills;
    }

    public bool IsBossSpawned()
    {
        return bossSpawned;
    }
}