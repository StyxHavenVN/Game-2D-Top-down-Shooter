using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    // Dictionary lưu trữ món đồ và số lượng stack của nó
    public Dictionary<ItemData, int> items = new Dictionary<ItemData, int>();

    [Header("Hiệu ứng Đặc biệt (Special Effects)")]
    public bool hasPierce = false;
    public bool hasBurn = false;
    public float lifestealPercent = 0f;

    // Hàm gọi khi người chơi dẫm lên vật phẩm
    public void AddItem(ItemData newItem)
    {
        // Nếu đã có đồ này rồi, tăng stack lên 1
        if (items.ContainsKey(newItem))
        {
            items[newItem]++;
            Debug.Log($"Đã cộng dồn (Stack): {newItem.itemName} x{items[newItem]}");
        }
        // Nếu chưa có, thêm vào kho với số lượng là 1
        else
        {
            items.Add(newItem, 1);
            Debug.Log($"Nhặt đồ mới: {newItem.itemName}");
        }

        // Sau khi nhặt, yêu cầu hệ thống tính toán lại toàn bộ sức mạnh
        CalculateTotalStats();
    }

    public void CalculateTotalStats()
    {
        float totalDamage = 10f; // Sát thương gốc của nhân vật
        float totalHealth = 100f; // Máu gốc

        // Reset các hiệu ứng trước khi tính lại
        hasPierce = false;
        hasBurn = false;
        lifestealPercent = 0f;

        // Quét toàn bộ kho đồ để cộng dồn chỉ số
        foreach (KeyValuePair<ItemData, int> entry in items)
        {
            ItemData item = entry.Key;
            int stackCount = entry.Value;

            // Công thức Risk of Rain: Chỉ số = Gốc (cái 1) + (Cộng thêm * (số lượng - 1))
            totalDamage += item.baseDamageBonus + (item.stackDamageBonus * (stackCount - 1));
            totalHealth += item.baseHealthBonus + (item.stackHealthBonus * (stackCount - 1));

            // Kích hoạt hiệu ứng dựa trên synergyTag
            if (item.synergyTag == "Pierce") hasPierce = true;
            if (item.synergyTag == "Burn") hasBurn = true;
            if (item.synergyTag == "Lifesteal") lifestealPercent += 0.15f * stackCount; // Mỗi món +15% hút máu
        }

        Debug.Log($"=== CHỈ SỐ HIỆN TẠI === Sát thương: {totalDamage} | Máu: {totalHealth}");

        // Cập nhật chỉ số này sang Script PlayerStats của bạn (Sẽ liên kết sau)
    }
}