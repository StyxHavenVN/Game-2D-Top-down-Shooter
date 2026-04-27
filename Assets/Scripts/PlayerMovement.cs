using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Animator anim;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    void FixedUpdate()
    {
        // Di chuyển nhân vật bằng phím WASD
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
                Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
                mouseWorldPos.z = 0f;

                facingDir = (mouseWorldPos - transform.position).normalized;
            }

            if (facingDir != Vector2.zero)
            {
                anim.SetFloat("Horizontal", facingDir.x);
                anim.SetFloat("Vertical", facingDir.y);
            }

            anim.SetFloat("Speed", moveInput.sqrMagnitude);
        }
    }
}