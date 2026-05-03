using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Xử lý di chuyển WASD của người chơi.
/// Hỗ trợ trạng thái Dash: khi đang dash, script này sẽ nhường
/// quyền điều khiển velocity cho PlayerDash.
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    [Header("Chỉ số Di chuyển")]
    public float speed = 5f;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Animator anim;
    private bool isDashing = false; // Được PlayerDash bật/tắt

    void Start()
    {
        rb   = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    void FixedUpdate()
    {
        // Khi đang Dash, PlayerDash sẽ tự điều khiển velocity — ta không ghi đè
        if (isDashing) return;

        rb.linearVelocity = moveInput * speed;
    }

    void Update()
    {
        if (anim != null)
        {
            Vector2 facingDir = moveInput;

            if (Mouse.current != null && Mouse.current.leftButton.isPressed)
            {
                Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
                Vector3 mouseWorldPos  = Camera.main.ScreenToWorldPoint(mouseScreenPos);
                mouseWorldPos.z = 0f;
                facingDir = (mouseWorldPos - transform.position).normalized;
            }

            if (facingDir != Vector2.zero)
            {
                anim.SetFloat("Horizontal", facingDir.x);
                anim.SetFloat("Vertical",   facingDir.y);
            }

            anim.SetFloat("Speed", moveInput.sqrMagnitude);
        }
    }

    // -------------------------------------------------------
    // API công khai để PlayerDash gọi
    // -------------------------------------------------------

    /// <summary>PlayerDash gọi hàm này để khoá/mở lại di chuyển thường.</summary>
    public void SetDashing(bool value) => isDashing = value;

    /// <summary>Trả về hướng di chuyển hiện tại (WASD input).</summary>
    public Vector2 GetMoveInput() => moveInput;
}