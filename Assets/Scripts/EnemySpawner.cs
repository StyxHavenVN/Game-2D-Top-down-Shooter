using UnityEngine;

/// <summary>
/// EnemySpawner cũ — KHÔNG CÒN SỬ DỤNG.
/// GameManager giờ quản lý toàn bộ việc spawn quái theo hệ thống Wave.
/// Script này được giữ lại để tương thích ngược (tránh lỗi tham chiếu).
/// GameManager sẽ tự động tắt script này khi Start().
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Danh sách các loại quái")]
    public GameObject[] enemyPrefabs;
    public Transform player;

    public float spawnRate = 3f;
    private float timer = 0f;

    [Header("Cài đặt Độ khó")]
    public float difficultyMultiplier = 1f;
    public float difficultyGrowthRate = 0.005f;
    public float maxDifficultyMultiplier = 5f;

    [Header("Cài đặt Spawn")]
    public float spawnDistance = 8f;
    public float minSpawnRate = 0.5f;
    public float spawnRateDecay = 0.05f;

    void Start()
    {
        // GameManager tự quản lý spawn wave — Script này bị vô hiệu hóa tự động
        if (GameManager.Instance != null)
        {
            Debug.Log("[EnemySpawner] Đã bị vô hiệu hóa — GameManager quản lý Wave.");
            enabled = false;
        }

        if (player == null)
        {
            GameObject pObj = GameObject.FindWithTag("Player");
            if (pObj == null) pObj = GameObject.Find("Player");
            if (pObj != null) player = pObj.transform;
        }
    }

    void Update()
    {
        // Fallback: nếu không có GameManager, vẫn spawn kiểu cũ
        if (player == null) return;

        timer += Time.deltaTime;

        if (difficultyMultiplier < maxDifficultyMultiplier)
        {
            difficultyMultiplier += Time.deltaTime * difficultyGrowthRate;
            difficultyMultiplier = Mathf.Min(difficultyMultiplier, maxDifficultyMultiplier);
        }

        if (timer >= spawnRate)
        {
            SpawnEnemy();
            timer = 0f;
            spawnRate = Mathf.Max(minSpawnRate, spawnRate - spawnRateDecay);
        }
    }

    void SpawnEnemy()
    {
        if (player == null) return;

        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        Vector2 spawnPosition = (Vector2)player.position + (randomDirection * spawnDistance);

        if (enemyPrefabs == null || enemyPrefabs.Length == 0) return;

        GameObject prefabToSpawn = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

        GameObject newEnemy = ObjectPool.Instance.GetEnemy(prefabToSpawn, spawnPosition);
        if (newEnemy == null) return;

        EnemyHealth enemyHealth = newEnemy.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            float baseMaxHP = enemyHealth.maxHealth;
            if (enemyHealth.enemyData != null)
                baseMaxHP = enemyHealth.enemyData.maxHP;

            enemyHealth.maxHealth = baseMaxHP * difficultyMultiplier;
            enemyHealth.ResetHealth();
        }
    }
}