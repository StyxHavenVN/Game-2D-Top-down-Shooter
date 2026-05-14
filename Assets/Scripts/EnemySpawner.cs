using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Danh sách quái có thể spawn")]
    public GameObject[] enemyPrefabs;

    [Header("Player")]
    public Transform player;

    [Header("Spawn")]
    public float spawnRate = 3f;
    public float spawnDistance = 8f;
    private float timer = 0f;

    [Header("Độ khó")]
    public float difficultyMultiplier = 1f;
    public float difficultyIncreaseRate = 0.01f;
    public float minSpawnRate = 0.5f;
    public float spawnRateDecrease = 0.05f;

    [Header("Tỉ lệ spawn")]
    [Range(0f, 100f)] public float meleeChance = 60f;
    [Range(0f, 100f)] public float rangedChance = 25f;
    [Range(0f, 100f)] public float exploderChance = 15f;

    void Start()
    {
        if (player == null)
        {
            GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");

            if (foundPlayer != null)
            {
                player = foundPlayer.transform;
            }
            else
            {
                Debug.LogError("[EnemySpawner] Không tìm thấy Player. Hãy gắn tag Player hoặc kéo Player vào Inspector.");
            }
        }
    }

    void Update()
    {
        timer += Time.deltaTime;

        difficultyMultiplier += Time.deltaTime * difficultyIncreaseRate;

        if (timer >= spawnRate)
        {
            SpawnEnemy();
            timer = 0f;

            spawnRate = Mathf.Max(minSpawnRate, spawnRate - spawnRateDecrease);
        }
    }

    void SpawnEnemy()
    {
        if (player == null) return;

        GameObject prefabToSpawn = GetRandomEnemyPrefab();

        if (prefabToSpawn == null)
        {
            Debug.LogError("[EnemySpawner] Chưa kéo enemy prefab vào danh sách Enemy Prefabs.");
            return;
        }

        Vector2 randomDirection = Random.insideUnitCircle.normalized;

        if (randomDirection == Vector2.zero)
        {
            randomDirection = Vector2.right;
        }

        Vector2 spawnPosition = (Vector2)player.position + randomDirection * spawnDistance;

        GameObject newEnemy = Instantiate(
            prefabToSpawn,
            spawnPosition,
            Quaternion.identity
        );

        EnemyBase enemyBase = newEnemy.GetComponent<EnemyBase>();

        if (enemyBase == null)
        {
            enemyBase = newEnemy.GetComponentInParent<EnemyBase>();
        }

        if (enemyBase != null)
        {
            enemyBase.maxHealth = Mathf.RoundToInt(enemyBase.maxHealth * difficultyMultiplier);

            newEnemy.name = enemyBase.enemyName + " Lv." + (difficultyMultiplier * 10).ToString("0");
        }
        else
        {
            Debug.LogWarning("[EnemySpawner] Prefab " + prefabToSpawn.name + " chưa có script kế thừa EnemyBase.");
        }
    }

    GameObject GetRandomEnemyPrefab()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            return null;
        }

        // Nếu bạn kéo đúng thứ tự:
        // 0 = MeleeEnemy
        // 1 = RangedEnemy
        // 2 = ExploderEnemy

        if (enemyPrefabs.Length < 3)
        {
            return enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        }

        float totalChance = meleeChance + rangedChance + exploderChance;

        if (totalChance <= 0)
        {
            return enemyPrefabs[0];
        }

        float roll = Random.Range(0f, totalChance);

        if (roll < meleeChance)
        {
            return enemyPrefabs[0]; // Melee
        }

        if (roll < meleeChance + rangedChance)
        {
            return enemyPrefabs[1]; // Ranged
        }

        return enemyPrefabs[2]; // Exploder
    }
}