using UnityEngine;

public class LootItem : MonoBehaviour
{
    public string itemName;
    public int bonusDamage;
    public int bonusHealth;

    void Start()
    {
        GenerateRandomItem();
    }

    void GenerateRandomItem()
    {
        // 1. Ngẫu nhiên tên món đồ
        string[] names = { "Kiếm Sắt Gỉ", "Rìu Chiến Binh", "Áo Da Sói", "Nhẫn Ma Thuật", "Giày Siêu Tốc" };
        itemName = names[Random.Range(0, names.Length)];

        // 2. Ngẫu nhiên chỉ số (Roll stats)
        bonusDamage = Random.Range(1, 15); // Sát thương từ 1 đến 14
        bonusHealth = Random.Range(10, 50); // Máu từ 10 đến 49

        // 3. Ngẫu nhiên độ hiếm (Đổi màu vật phẩm)
        int rarityRoll = Random.Range(0, 100);
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        if (rarityRoll < 50) // 50% ra đồ Thường (Trắng)
        {
            sr.color = Color.white;
            itemName = "Đồ Thường: " + itemName;
        }
        else if (rarityRoll < 85) // 35% ra đồ Hiếm (Xanh dương)
        {
            sr.color = Color.cyan;
            itemName = "Đồ Hiếm: " + itemName;
            bonusDamage += 5; // Đồ hiếm được cộng thêm chỉ số
        }
        else // 15% ra đồ Thần Thoại (Vàng/Cam)
        {
            sr.color = new Color(1f, 0.5f, 0f); // Màu cam
            itemName = "Đồ THẦN THOẠI: " + itemName;
            bonusDamage += 15;
            bonusHealth += 30;
        }

        // In ra Console để bạn kiểm tra món đồ vừa rớt
        Debug.Log($"Rớt đồ: {itemName} | Sát thương: +{bonusDamage} | Máu: +{bonusHealth}");
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        // Kiểm tra xem người dẫm lên có phải là nhân vật chính (Player) không
        if (collision.name == "Player")
        {
            // Báo lên Console là đã nhặt được
            Debug.Log($"ĐÃ NHẶT: {itemName} (Sát thương: +{bonusDamage}, Máu: +{bonusHealth})");

            // Ở game thực tế, chỗ này bạn sẽ code thêm hàm cộng chỉ số cho Player
            // ...

            // Nhặt xong thì xóa món đồ khỏi mặt đất
            Destroy(gameObject);
        }
    }
}
