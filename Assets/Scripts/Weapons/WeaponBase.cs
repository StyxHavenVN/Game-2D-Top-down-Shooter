using UnityEngine;

/// <summary>
/// Lớp gốc cho tất cả các loại vũ khí.
/// Tất cả súng hay kiếm đều phải kế thừa lớp này.
/// </summary>
public abstract class WeaponBase : MonoBehaviour
{
    [Header("Dữ liệu Vũ khí")]
    public WeaponData weaponData; // Kéo file ScriptableObject tương ứng vào đây

    [Header("Tham chiếu Hệ thống")]
    protected ObjectPool pool;
    protected AudioManager audioManager;
    protected InventoryManager inventory;

    protected float nextAttackTime = 0f;

    protected virtual void Start()
    {
        pool = ObjectPool.Instance;
        audioManager = AudioManager.Instance;
        inventory = GetComponentInParent<InventoryManager>();
    }

    /// <summary>
    /// Kiểm tra xem vũ khí có sẵn sàng bắn chưa (dựa vào fireRate)
    /// </summary>
    public bool CanAttack()
    {
        return Time.time >= nextAttackTime;
    }

    /// <summary>
    /// Hàm bắn/chém trừu tượng. Các vũ khí con bắt buộc phải viết đè (override) hàm này.
    /// </summary>
    public abstract void Attack(Vector2 direction, float angle);
}
