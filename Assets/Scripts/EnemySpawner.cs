using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
<<<<<<< Updated upstream
    public GameObject enemyPrefab; // Chứa con quái mẫu
    public Transform player;       // Chứa vị trí người chơi
=======
    [Header("Danh sách các loại quái")]
    public GameObject[] enemyPrefabs;
    public Transform player;     
>>>>>>> Stashed changes

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

<<<<<<< Updated upstream
        // Sinh ra con quái
        GameObject newEnemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
=======
        GameObject prefabToSpawn = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

        GameObject newEnemy = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
>>>>>>> Stashed changes

        EnemyHealth enemyHealth = newEnemy.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
<<<<<<< Updated upstream
            // Nhân máu gốc (100) với độ khó. Ví dụ hệ số 1.5 thì quái sẽ có 150 máu.
            enemyHealth.maxHealth = Mathf.RoundToInt(enemyHealth.maxHealth * difficultyMultiplier);

            // Đổi tên nó một chút cho ngầu để bạn dễ theo dõi ở Console
=======
            enemyHealth.maxHealth = enemyHealth.maxHealth * difficultyMultiplier;
>>>>>>> Stashed changes
            newEnemy.name = "Enemy Lv." + (difficultyMultiplier * 10).ToString("0");
        }
    }
}