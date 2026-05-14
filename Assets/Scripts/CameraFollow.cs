using UnityEngine;
using UnityEngine.InputSystem;

public class CameraFollow : MonoBehaviour
{
    [Header("Di Chuyen Camera (Follow)")]
    public Transform target;
    public float smoothTime = 0.1f;
    public Vector3 offset = new Vector3(0, 0, -10f);

    private Vector3 velocity = Vector3.zero;

    [Header("Thu Phong Camera (Scroll Zoom)")]
    public float zoomSpeed = 10f;
    public float zoomMultiplier = 0.01f;
    public float minZoom = 2f;
    public float maxZoom = 15f;

    private Camera cam;
    private float targetZoom;

    void Start()
    {
        cam = GetComponent<Camera>();
        targetZoom = cam.orthographicSize;

        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (player != null)
            {
                target = player.transform;
            }
            else
            {
                Debug.LogError("[CameraFollow] Không tìm thấy Player. Hãy gắn Tag Player hoặc kéo Player vào Target.");
            }
        }

        // Cho camera nhảy ngay tới Player lúc bắt đầu, tránh bị trôi từ vị trí cũ tới
        if (target != null)
        {
            transform.position = target.position + offset;
        }
    }

    void LateUpdate()
    {
        if (target != null)
        {
            Vector3 targetPosition = target.position + offset;

            transform.position = Vector3.SmoothDamp(
                transform.position,
                targetPosition,
                ref velocity,
                smoothTime
            );
        }

        if (Mouse.current != null)
        {
            float scrollData = Mouse.current.scroll.ReadValue().y;

            if (scrollData != 0)
            {
                targetZoom -= scrollData * zoomMultiplier;
                targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
            }

            cam.orthographicSize = Mathf.Lerp(
                cam.orthographicSize,
                targetZoom,
                Time.deltaTime * zoomSpeed
            );
        }
    }
}