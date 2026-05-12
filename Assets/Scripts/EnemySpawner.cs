using UnityEngine;

public class EnemySpawner : MonoBehaviour
{

    [Header("Danh sách các loại quái")]
    public GameObject[] enemyPrefabs;
    public Transform player;     

    public float spawnRate = 3f; 
    private float timer = 0f;   

    public float difficultyMultiplier = 1f; 

    void Start()
    {
        if (player == null) player = GameObject.Find("Player").transform;
    }

    void Update()
    {
        timer += Time.deltaTime;
        difficultyMultiplier += Time.deltaTime * 0.01f;
        if (timer >= spawnRate)
        {
            SpawnEnemy();
            timer = 0f; 

            spawnRate = Mathf.Max(0.5f, spawnRate - 0.05f);
        }
    }

    void SpawnEnemy()
    {
        if (player == null) return;

        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        Vector2 spawnPosition = (Vector2)player.position + (randomDirection * 8f);

        GameObject prefabToSpawn = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

        // LẤY QUÁI TỪ KHO (Object Pool) thay vì Instantiate
        GameObject newEnemy = ObjectPool.Instance.GetEnemy(prefabToSpawn, spawnPosition);

        EnemyHealth enemyHealth = newEnemy.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.maxHealth = enemyHealth.maxHealth * difficultyMultiplier;

            // Reset máu (rất quan trọng — quái lấy ra từ Pool có thể đang máu 0 từ lần chết trước)
            enemyHealth.ResetHealth();

            // Đổi tên nó một chút cho ngầu để bạn dễ theo dõi ở Console
            newEnemy.name = "Enemy Lv." + (difficultyMultiplier * 10).ToString("0");
        }
    }
}