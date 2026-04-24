using UnityEngine;
using UnityEngine.InputSystem;

public class CameraFollow : MonoBehaviour
{
    [Header("Di Chuyen Camera (Follow)")]
    public Transform target; // Kéo Player vào đây
    public float smoothTime = 100f; // Độ trễ (càng lớn thì camera đi theo càng mượt/chậm)
    public Vector3 offset = new Vector3(0, 0, -10f); // Giữ khoảng cách trục Z để không bị lỗi tàng hình
    private Vector3 velocity = Vector3.zero;

    [Header("Thu Phong Camera (Scroll Zoom)")]
    public float zoomSpeed = 10f; // Tốc độ nội suy mượt của Zoom
    public float zoomMultiplier = 0.01f; // Nhân với số siêu nhỏ để ghìm lực cuộn chuột của hệ thống Mới
    public float minZoom = 2f; // Zoom gần nhất
    public float maxZoom = 15f; // Zoom xa nhất

    private Camera cam;
    private float targetZoom;

    void Start()
    {
        cam = GetComponent<Camera>();
        targetZoom = cam.orthographicSize; // Lấy giá trị zoom ban đầu làm gốc
    }

    // Luôn dùng LateUpdate cho Camera để nó chạy sau khi nhân vật đã di chuyển xong
    void LateUpdate()
    {
        // 1. XỬ LÝ ĐUỔI THEO NHÂN VẬT MƯỢT MÀ
        if (target != null)
        {
            Vector3 targetPosition = target.position + offset;
            // SmoothDamp giúp camera lướt đi êm ái như dùng gimbal
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        }

        // 2. XỬ LÝ LĂN CHUỘT THU PHÓNG MƯỢT MÀ
        if (Mouse.current != null)
        {
            float scrollData = Mouse.current.scroll.ReadValue().y;

            // Tính toán mức zoom mục tiêu (nếu lăn chuột)
            if (scrollData != 0)
            {
                targetZoom -= scrollData * zoomMultiplier;
                targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom); // Khóa giới hạn zoom
            }

            // Lerp giúp camera thu phóng từ từ tới điểm mục tiêu thay vì giật cục lập tức
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetZoom, Time.deltaTime * zoomSpeed);
        }
    }
}