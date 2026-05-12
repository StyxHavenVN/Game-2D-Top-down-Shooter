using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Hệ thống Object Pool giúp tái sử dụng đạn và quái vật thay vì dùng Instantiate/Destroy liên tục.
/// Việc này giúp tăng FPS đáng kể khi game có nhiều đạn/quái.
///
/// CÁCH DÙNG:
///   Lấy đạn:  GameObject b = ObjectPool.Instance.GetBullet(pos, rot);
///   Trả đạn:  ObjectPool.Instance.ReturnBullet(b);
///   Lấy quái: GameObject e = ObjectPool.Instance.GetEnemy(prefab, pos);
///   Trả quái: ObjectPool.Instance.ReturnEnemy(e);
/// </summary>
public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance { get; private set; }

    // ── BULLET POOL ────────────────────────────────────────────
    [Header("Bullet Pool Settings")]
    public GameObject bulletPrefab;
    public int bulletPoolSize = 50;

    // Hàng đợi chứa các viên đạn đang KHÔNG được sử dụng (nằm trong kho)
    private Queue<GameObject> bulletPool = new Queue<GameObject>();

    // ── ENEMY POOL ─────────────────────────────────────────────
    [Header("Enemy Pool Settings")]
    public GameObject[] enemyPrefabs;       // Kéo các loại quái vào đây (BasicEnemy, RangedEnemy)
    public int enemyPoolSizePerType = 20;   // Mỗi loại quái tạo sẵn bao nhiêu con

    // Dictionary: mỗi loại Prefab có 1 hàng đợi riêng
    private Dictionary<string, Queue<GameObject>> enemyPools = new Dictionary<string, Queue<GameObject>>();
    // Lưu tên prefab gốc lên mỗi con quái để biết trả về đúng hàng đợi
    private Dictionary<GameObject, string> enemyPrefabMap = new Dictionary<GameObject, string>();

    // ── AWAKE ──────────────────────────────────────────────────
    private void Awake()
    {
        // Singleton pattern: Chỉ cho phép tồn tại 1 ObjectPool duy nhất
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Tạo sẵn kho đạn và kho quái
        InitBulletPool();
        InitEnemyPool();
    }

    // ════════════════════════════════════════════════════════════
    //  BULLET POOL
    // ════════════════════════════════════════════════════════════

    private void InitBulletPool()
    {
        if (bulletPrefab == null) return;

        for (int i = 0; i < bulletPoolSize; i++)
        {
            GameObject obj = Instantiate(bulletPrefab);
            obj.SetActive(false);
            obj.transform.SetParent(transform);
            bulletPool.Enqueue(obj);
        }
    }

    /// <summary>
    /// Lấy 1 viên đạn ra bắn, thay cho Instantiate.
    /// </summary>
    public GameObject GetBullet(Vector3 position, Quaternion rotation)
    {
        if (bulletPrefab == null)
        {
            Debug.LogError("[ObjectPool] Lỗi: Chưa cấu hình bulletPrefab trong ObjectPool!");
            return null;
        }

        GameObject bullet;

        if (bulletPool.Count > 0)
        {
            bullet = bulletPool.Dequeue();
        }
        else
        {
            // Kho hết đạn → tạo mới thêm 1 viên
            bullet = Instantiate(bulletPrefab);
            bullet.transform.SetParent(transform);
        }

        bullet.SetActive(true);
        bullet.transform.position = position;
        bullet.transform.rotation = rotation;

        // Reset vận tốc để viên đạn cũ không bị "nhớ" hướng bay cũ
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        // Reset trạng thái hiệu ứng đặc biệt
        BulletDamage bd = bullet.GetComponent<BulletDamage>();
        if (bd != null)
        {
            bd.isPierce = false;
            bd.isBurn = false;
            bd.ResetBullet(); // Gọi hàm reset timer
        }

        return bullet;
    }

    /// <summary>
    /// Trả viên đạn về kho, thay cho Destroy.
    /// </summary>
    public void ReturnBullet(GameObject bullet)
    {
        bullet.SetActive(false);

        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        bulletPool.Enqueue(bullet);
    }

    // ════════════════════════════════════════════════════════════
    //  ENEMY POOL
    // ════════════════════════════════════════════════════════════

    private void InitEnemyPool()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0) return;

        foreach (GameObject prefab in enemyPrefabs)
        {
            if (prefab == null) continue;

            string key = prefab.name;
            Queue<GameObject> pool = new Queue<GameObject>();

            for (int i = 0; i < enemyPoolSizePerType; i++)
            {
                GameObject obj = Instantiate(prefab);
                obj.SetActive(false);
                obj.transform.SetParent(transform);
                pool.Enqueue(obj);
                enemyPrefabMap[obj] = key; // Ghi nhớ con này thuộc loại nào
            }

            enemyPools[key] = pool;
        }
    }

    /// <summary>
    /// Lấy 1 con quái từ kho ra, thay cho Instantiate.
    /// Truyền vào đúng cái Prefab gốc để hệ thống biết lấy từ hàng đợi nào.
    /// </summary>
    public GameObject GetEnemy(GameObject prefab, Vector3 position)
    {
        if (prefab == null)
        {
            Debug.LogError("[ObjectPool] Lỗi: Cố gắng lấy quái với prefab bị NULL!");
            return null;
        }

        string key = prefab.name;
        GameObject enemy;

        if (enemyPools.ContainsKey(key) && enemyPools[key].Count > 0)
        {
            enemy = enemyPools[key].Dequeue();
        }
        else
        {
            // Kho hết → tạo mới thêm 1 con
            enemy = Instantiate(prefab);
            enemy.transform.SetParent(transform);
            enemyPrefabMap[enemy] = key;
        }

        enemy.transform.position = position;
        enemy.transform.rotation = Quaternion.identity;
        enemy.SetActive(true);

        return enemy;
    }

    /// <summary>
    /// Trả 1 con quái về kho, thay cho Destroy.
    /// </summary>
    public void ReturnEnemy(GameObject enemy)
    {
        enemy.SetActive(false);

        // Reset vận tốc
        Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        // Trả về đúng hàng đợi
        if (enemyPrefabMap.ContainsKey(enemy))
        {
            string key = enemyPrefabMap[enemy];
            if (enemyPools.ContainsKey(key))
            {
                enemyPools[key].Enqueue(enemy);
                return;
            }
        }

        // Nếu không tìm thấy hàng đợi, tắt luôn (an toàn)
        Debug.LogWarning("[ObjectPool] Không tìm thấy pool cho quái: " + enemy.name);
    }
}
