using UnityEngine;

// Dòng này giúp bạn có thể click chuột phải trong Unity để tạo Item mới rất nhanh
[CreateAssetMenu(fileName = "New Item", menuName = "Roguelite/Item Data")]
public class ItemData : ScriptableObject
{
    public string itemName;
    [TextArea] public string description;
    public Sprite itemIcon; // Bạn sẽ gắn hình ảnh bạn vẽ vào đây

    [Header("Chỉ số Cộng thêm (Risk of Rain Style)")]
    public float baseDamageBonus;  // Cộng khi nhặt cái đầu tiên
    public float stackDamageBonus; // Cộng thêm cho mỗi cái nhặt thêm (Stack)

    public float baseHealthBonus;
    public float stackHealthBonus;

    [Header("Hệ thống Đột biến (Isaac Style)")]
    public string synergyTag; // Đánh dấu loại hiệu ứng (VD: "Explosive", "Bouncing")
}