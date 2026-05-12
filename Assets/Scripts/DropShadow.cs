using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class DropShadow : MonoBehaviour
{
    [Header("Cài đặt Bóng")]
    public Vector2 shadowOffset = new Vector2(0.2f, -0.3f); // Độ lệch của bóng
    public Color shadowColor = new Color(0f, 0f, 0f, 0.4f); // Màu bóng (Đen, hơi trong suốt)
    public Vector3 shadowScale = new Vector3(1f, -0.5f, 1f); // Bóp lùn và lật ngược bóng xuống đất
    public float skewAngle = -20f; // Góc nghiêng của bóng

    private SpriteRenderer casterRenderer;
    private SpriteRenderer shadowRenderer;
    private Transform shadowTransform;

    void Start()
    {
        casterRenderer = GetComponent<SpriteRenderer>();

        // 1. Sinh ra một vật thể con để chứa cái bóng
        GameObject shadowObject = new GameObject("Shadow_Sprite");
        shadowTransform = shadowObject.transform;
        shadowTransform.parent = transform;

        // 2. Chỉnh vị trí, kích thước và độ nghiêng cho giống bóng nắng
        shadowTransform.localPosition = shadowOffset;
        shadowTransform.localScale = shadowScale;
        shadowTransform.localRotation = Quaternion.Euler(0, 0, skewAngle);

        // 3. Copy hình ảnh của vật chủ, tô màu đen
        shadowRenderer = shadowObject.AddComponent<SpriteRenderer>();
        shadowRenderer.sprite = casterRenderer.sprite;
        shadowRenderer.color = shadowColor;

        // 4. Bắt buộc bóng phải nằm DƯỚI vật chủ
        shadowRenderer.sortingLayerName = casterRenderer.sortingLayerName;
        shadowRenderer.sortingOrder = casterRenderer.sortingOrder - 1;
    }

    void Update()
    {
        // Liên tục cập nhật hình dáng bóng (Rất cần thiết nếu nhân vật đang chạy/hoạt ảnh)
        if (shadowRenderer.sprite != casterRenderer.sprite)
        {
            shadowRenderer.sprite = casterRenderer.sprite;
        }
    }
}