using UnityEngine;

/// <summary>
/// Quản lý việc rơi đồ ngẫu nhiên khi quái chết.
/// Áp dụng Singleton để dễ dàng gọi từ bất kỳ đâu.
/// </summary>
public class ItemDropManager : MonoBehaviour
{
    public static ItemDropManager Instance { get; private set; }

    [Header("Cài đặt rơi đồ")]
    [Tooltip("Kéo Prefab hộp quà rỗng (chứa ItemPickup) vào đây")]
    public GameObject itemPickupPrefab; 
    
    [Tooltip("Danh sách các Item có thể rớt ra (Kéo Pierce, Burn, Lifesteal... vào đây)")]
    public ItemData[] possibleItems;

    [Tooltip("Tỷ lệ rớt đồ (0.1 = 10% mỗi khi 1 con quái chết)")]
    [Range(0f, 1f)]
    public float dropChance = 0.05f; // 5% mặc định cho game khó

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Hàm này được gọi khi một con quái chết.
    /// </summary>
    public void TryDropItem(Vector3 dropPosition)
    {
        // Kiểm tra xem đã thiết lập đầy đủ Prefab và danh sách chưa
        if (itemPickupPrefab == null || possibleItems == null || possibleItems.Length == 0)
            return;

        // Đổ xúc xắc (Random.value trả về một số ngẫu nhiên từ 0.0 đến 1.0)
        if (Random.value <= dropChance)
        {
            // Chọn ngẫu nhiên 1 món đồ trong danh sách
            ItemData randomItem = possibleItems[Random.Range(0, possibleItems.Length)];

            // Tung độ lệch nhỏ để đồ rớt ra không đè lên ngọc EXP
            Vector3 randomOffset = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0f);

            // Tạo hộp quà rớt ra
            GameObject lootObj = Instantiate(itemPickupPrefab, dropPosition + randomOffset, Quaternion.identity);
            
            // Nhét dữ liệu của món đồ vào hộp quà
            ItemPickup pickup = lootObj.GetComponent<ItemPickup>();
            if (pickup != null)
            {
                pickup.itemData = randomItem;
            }
            
            Debug.Log($"[ItemDropManager] Rớt đồ: {randomItem.itemName}!");
        }
    }
}
