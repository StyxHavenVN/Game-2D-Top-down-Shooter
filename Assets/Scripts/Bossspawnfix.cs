using System.Collections;
using UnityEngine;

/// <summary>
/// BossSpawnFix - Drop script này vào cùng GameObject với GameManager.
/// Script sẽ tự vá lỗi bossPrefab null và spawn boss đúng vị trí gần player.
/// 
/// CÁCH DÙNG:
///   1. Đặt Boss prefab vào thư mục Assets/Resources/ (tạo folder nếu chưa có)
///   2. Đổi tên file prefab thành "Boss" (hoặc thay BOSS_RESOURCE_PATH bên dưới)
///   3. Attach script này vào GameManager GameObject
///   4. KHÔNG cần sửa GameManager.cs gốc
/// </summary>
public class BossSpawnFix : MonoBehaviour
{
    [Header("Boss Resource Path")]
    [Tooltip("Tên prefab trong folder Assets/Resources/ (không cần extension)")]
    public string bossResourcePath = "Boss";

    [Header("Spawn Settings")]
    [Tooltip("Khoảng cách spawn boss từ player (đủ xa để player thấy boss đi vào)")]
    public float spawnDistanceFromPlayer = 8f;
    [Tooltip("Spawn boss ở cạnh nào của màn hình (true = random, false = phía trên)")]
    public bool randomSpawnSide = true;

    [Header("Debug")]
    public bool showDebugLog = true;

    // Reference tự tìm
    private GameManager _gm;
    private GameObject _bossPrefabRef;

    void Awake()
    {
        _gm = GetComponent<GameManager>();
        if (_gm == null)
        {
            Debug.LogError("[BossSpawnFix] Không tìm thấy GameManager trên cùng GameObject!");
            return;
        }

        // Load boss prefab từ Resources
        _bossPrefabRef = Resources.Load<GameObject>(bossResourcePath);

        if (_bossPrefabRef == null)
        {
            Debug.LogError($"[BossSpawnFix] Không tìm thấy prefab tại Resources/{bossResourcePath}. " +
                           "Hãy chắc chắn file prefab nằm trong Assets/Resources/ và tên đúng!");
        }
        else
        {
            if (showDebugLog)
                Debug.Log($"[BossSpawnFix] Đã load boss prefab: {_bossPrefabRef.name}");

            // Gán vào GameManager nếu field bossPrefab đang null
            PatchGameManagerBossPrefab();
        }
    }

    /// <summary>
    /// Dùng reflection để gán bossPrefab vào GameManager mà không cần sửa code gốc.
    /// Hoạt động với bất kỳ tên field nào (bossPrefab, BossPrefab, boss_prefab, v.v.)
    /// </summary>
    private void PatchGameManagerBossPrefab()
    {
        var gmType = _gm.GetType();

        // Thử các tên field phổ biến
        string[] possibleFieldNames = { "bossPrefab", "BossPrefab", "boss_prefab", "bossObject", "BossObject" };

        foreach (var fieldName in possibleFieldNames)
        {
            var field = gmType.GetField(fieldName,
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);

            if (field != null && field.FieldType == typeof(GameObject))
            {
                var currentValue = field.GetValue(_gm) as GameObject;
                if (currentValue == null)
                {
                    field.SetValue(_gm, _bossPrefabRef);
                    if (showDebugLog)
                        Debug.Log($"[BossSpawnFix] Đã gán bossPrefab vào field '{fieldName}' của GameManager.");
                }
                else
                {
                    if (showDebugLog)
                        Debug.Log($"[BossSpawnFix] Field '{fieldName}' đã có prefab: {currentValue.name}. Không cần patch.");
                }
                return;
            }
        }

        // Nếu không tìm thấy field → dùng manual spawn
        Debug.LogWarning("[BossSpawnFix] Không tìm thấy field bossPrefab trong GameManager. " +
                         "Dùng chế độ Manual Spawn thay thế. Gọi SpawnBossManually() khi cần.");
    }

    /// <summary>
    /// Spawn boss thủ công gần player - gọi từ GameManager khi đủ kill count.
    /// Hoặc để script này tự listen qua MonitorKillCount().
    /// </summary>
    public GameObject SpawnBossManually()
    {
        if (_bossPrefabRef == null)
        {
            Debug.LogError("[BossSpawnFix] bossPrefab null, không thể spawn!");
            return null;
        }

        Vector3 spawnPos = GetSpawnPositionNearPlayer();
        GameObject boss = Instantiate(_bossPrefabRef, spawnPos, Quaternion.identity);

        if (showDebugLog)
            Debug.Log($"[BossSpawnFix] Boss spawned tại {spawnPos}");

        return boss;
    }

    private Vector3 GetSpawnPositionNearPlayer()
    {
        Transform player = FindAnyObjectByType<PlayerMovement>()?.transform;
        Vector3 center = player != null ? player.position : Vector3.zero;

        if (!randomSpawnSide)
        {
            // Spawn phía trên player
            return center + new Vector3(0, spawnDistanceFromPlayer, 0);
        }

        // Spawn ngẫu nhiên 1 trong 4 cạnh
        int side = Random.Range(0, 4);
        switch (side)
        {
            case 0: return center + new Vector3(0, spawnDistanceFromPlayer, 0);   // Trên
            case 1: return center + new Vector3(0, -spawnDistanceFromPlayer, 0);  // Dưới
            case 2: return center + new Vector3(-spawnDistanceFromPlayer, 0, 0);  // Trái
            default: return center + new Vector3(spawnDistanceFromPlayer, 0, 0);  // Phải
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Context menu để test spawn boss ngay trong Play Mode
    /// Click chuột phải vào script trong Inspector → "Test Spawn Boss"
    /// </summary>
    [ContextMenu("Test Spawn Boss")]
    void TestSpawnBoss()
    {
        SpawnBossManually();
    }
#endif
}