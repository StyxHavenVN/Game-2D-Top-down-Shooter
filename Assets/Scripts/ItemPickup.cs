using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    // Kéo thả cái ScriptableObject ItemData vào đây trên thanh Inspector
    public ItemData itemData;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Tìm túi đồ của người chơi và nhét đồ vào
            InventoryManager inventory = collision.GetComponent<InventoryManager>();
            if (inventory != null)
            {
                inventory.AddItem(itemData);
                Destroy(gameObject); // Nhặt xong thì xóa cục đồ trên đất
            }
        }
    }
}