using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab; // Chứa con quái mẫu
    public Transform player;       // Chứa vị trí người chơi

    public float spawnRate = 3f;   // Cứ 3 giây đẻ 1 con
    private float timer = 0f;      // Đồng hồ đếm giờ

    public float difficultyMultiplier = 1f; // Hệ số độ khó ban đầu là x1

    void Start()
    {
        if (player == null) player = GameObject.Find("Player").transform;
    }

    void Update()
    {
        // 1. Hệ thống đếm thời gian
        timer += Time.deltaTime;

        // 2. Làm quái mạnh lên theo thời gian (Mỗi giây tăng 1% máu)
        difficultyMultiplier += Time.deltaTime * 0.01f;

        // 3. Đến giờ thì đẻ quái
        if (timer >= spawnRate)
        {
            SpawnEnemy();
            timer = 0f; // Reset đồng hồ

            // Ép người chơi: Càng về sau quái đẻ càng nhanh (Nhanh nhất là 0.5s/con)
            spawnRate = Mathf.Max(0.5f, spawnRate - 0.05f);
        }
    }

    void SpawnEnemy()
    {
        if (player == null) return;

        // Tính toán vị trí đẻ quái ngẫu nhiên xung quanh người chơi (cách khoảng 8 mét)
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        Vector2 spawnPosition = (Vector2)player.position + (randomDirection * 8f);

        // Sinh ra con quái
        GameObject newEnemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

        // Cường hóa con quái vừa sinh ra dựa theo hệ số độ khó hiện tại
        Health enemyHealth = newEnemy.GetComponent<Health>();
        if (enemyHealth != null)
        {
            // Nhân máu gốc (100) với độ khó. Ví dụ hệ số 1.5 thì quái sẽ có 150 máu.
            enemyHealth.maxHealth = Mathf.RoundToInt(enemyHealth.maxHealth * difficultyMultiplier);

            // Đổi tên nó một chút cho ngầu để bạn dễ theo dõi ở Console
            newEnemy.name = "Enemy Lv." + (difficultyMultiplier * 10).ToString("0");
        }
    }
}