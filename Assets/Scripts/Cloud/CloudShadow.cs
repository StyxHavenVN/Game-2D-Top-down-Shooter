using UnityEngine;

public class CloudShadow : MonoBehaviour
{
    [Header("Tốc độ gió (Tăng lên nếu mây bay chậm)")]
    public float scrollSpeedX = 0.5f; // Đã tăng tốc độ lên gấp 10 lần
    public float scrollSpeedY = 0.2f;

    private Material mat;
    private Vector2 windOffset;

    private Transform camTransform;
    private float scaleX;
    private float scaleY;

    void Start()
    {
        // Lấy Material
        mat = GetComponent<Renderer>().material;

        // Tìm Camera
        camTransform = Camera.main.transform;

        // Lấy kích thước rèm
        scaleX = transform.localScale.x;
        scaleY = transform.localScale.y;
    }

    void Update()
    {
        // 1. Tính toán sức gió (thời gian trôi)
        windOffset.x += scrollSpeedX * Time.deltaTime;
        windOffset.y += scrollSpeedY * Time.deltaTime;

        // 2. Tính toán độ trượt của Camera
        float camOffsetX = camTransform.position.x / scaleX;
        float camOffsetY = camTransform.position.y / scaleY;

        // 3. Kết hợp lại
        Vector2 finalOffset = new Vector2(camOffsetX + windOffset.x, camOffsetY + windOffset.y);

        // Lệnh này an toàn và tương thích tốt hơn với URP
        mat.mainTextureOffset = finalOffset;
    }
}