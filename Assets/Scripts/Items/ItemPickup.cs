using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    // Kéo thả cái ScriptableObject ItemData vào đây trên thanh Inspector
    public ItemData itemData;

    void Start()
    {
        // Tự động gán hình ảnh của vật phẩm lên SpriteRenderer để hiển thị
        if (itemData != null && itemData.itemIcon != null)
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null) sr.sprite = itemData.itemIcon;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Tìm túi đồ của người chơi và nhét đồ vào
            InventoryManager inventory = collision.GetComponent<InventoryManager>();
            if (inventory != null)
            {
                inventory.AddItem(itemData);

                // Phát âm thanh nhặt đồ
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlayPickupItem();

                Destroy(gameObject); // Nhặt xong thì xóa cục đồ trên đất
            }
        }
    }
}