using UnityEngine;

/// <summary>
/// BossSpawnFix - Spawn boss giống EnemySpawner: random quanh Player.
/// Gắn script này vào GameManager hoặc object quản lý boss.
/// </summary>
public class BossSpawnFix : MonoBehaviour
{
    [Header("Boss Prefab")]
    public GameObject bossPrefab;

    [Header("Nếu không kéo prefab, script sẽ load từ Resources")]
    public string bossResourcePath = "Boss";

    [Header("Spawn Settings")]
    public float spawnDistanceFromPlayer = 8f;

    [Header("Debug")]
    public bool showDebugLog = true;
    public KeyCode testSpawnKey = KeyCode.B;

    private GameManager gameManager;

    void Awake()
    {
        gameManager = GetComponent<GameManager>();

        if (bossPrefab == null)
        {
            bossPrefab = Resources.Load<GameObject>(bossResourcePath);
        }

        if (bossPrefab == null)
        {
            Debug.LogError("[BossSpawnFix] Chưa có Boss Prefab. Kéo Boss prefab vào ô bossPrefab hoặc đặt tại Assets/Resources/Boss.prefab.");
            return;
        }

        if (showDebugLog)
        {
            Debug.Log("[BossSpawnFix] Đã tìm thấy Boss Prefab: " + bossPrefab.name);
        }

        PatchGameManagerBossPrefab();
    }

    void Update()
    {
        if (Input.GetKeyDown(testSpawnKey))
        {
            SpawnBossManually();
        }
    }

    private void PatchGameManagerBossPrefab()
    {
        if (gameManager == null) return;
        if (bossPrefab == null) return;

        var gmType = gameManager.GetType();

        string[] possibleFieldNames =
        {
            "bossPrefab",
            "BossPrefab",
            "boss_prefab",
            "bossObject",
            "BossObject"
        };

        foreach (string fieldName in possibleFieldNames)
        {
            var field = gmType.GetField(
                fieldName,
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance
            );

            if (field != null && field.FieldType == typeof(GameObject))
            {
                GameObject currentValue = field.GetValue(gameManager) as GameObject;

                if (currentValue == null)
                {
                    field.SetValue(gameManager, bossPrefab);

                    if (showDebugLog)
                    {
                        Debug.Log("[BossSpawnFix] Đã gán Boss Prefab vào GameManager field: " + fieldName);
                    }
                }

                return;
            }
        }

        if (showDebugLog)
        {
            Debug.LogWarning("[BossSpawnFix] Không tìm thấy field bossPrefab trong GameManager.");
        }
    }

    public GameObject SpawnBossManually()
    {
        if (bossPrefab == null)
        {
            Debug.LogError("[BossSpawnFix] bossPrefab đang null, không thể spawn boss.");
            return null;
        }

        Vector3 spawnPos = GetSpawnPositionLikeEnemy();

        GameObject boss = Instantiate(bossPrefab, spawnPos, Quaternion.identity);
        boss.name = "Boss";

        if (showDebugLog)
        {
            Debug.Log("[BossSpawnFix] Boss spawned tại: " + spawnPos);
        }

        return boss;
    }

    private Vector3 GetSpawnPositionLikeEnemy()
    {
        GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");

        if (foundPlayer == null)
        {
            Debug.LogWarning("[BossSpawnFix] Không tìm thấy Player. Boss spawn tại Vector3.zero.");
            return Vector3.zero;
        }

        Vector2 randomDirection = Random.insideUnitCircle.normalized;

        if (randomDirection == Vector2.zero)
        {
            randomDirection = Vector2.right;
        }

        Vector2 spawnPosition = (Vector2)foundPlayer.transform.position + randomDirection * spawnDistanceFromPlayer;

        return new Vector3(spawnPosition.x, spawnPosition.y, 0f);
    }

#if UNITY_EDITOR
    [ContextMenu("Test Spawn Boss")]
    private void TestSpawnBoss()
    {
        SpawnBossManually();
    }
#endif
}