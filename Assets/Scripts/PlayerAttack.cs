using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [Header("Kho Vũ Khí (Kéo Prefabs vào đây)")]
    public GameObject pistolBulletPrefab;   // Viên đạn súng lục
    public GameObject shotgunBulletPrefab;  // Viên đạn shotgun
    public GameObject swordSlashPrefab;     // Hiệu ứng nhát chém (Cận chiến)

    [Header("Chỉ số Súng Lục (Spam)")]
    public float pistolForce = 20f;
    public float pistolCooldown = 0.2f; // Bắn rất nhanh (0.2s/viên)

    [Header("Chỉ số Shotgun (Bắn chùm)")]
    public float shotgunForce = 15f;
    public float shotgunCooldown = 0.8f; // Bắn chậm hơn
    public int pelletsCount = 5; // Số viên đạn bắn ra cùng lúc
    public float spreadAngle = 45f; // Góc tỏa ra của đạn chùm

    [Header("Chỉ số Kiếm (Cận chiến)")]
    public float swordCooldown = 0.5f;
    public float swordRange = 1.2f; // Khoảng cách chém tính từ nhân vật

    // Bộ đếm thời gian để kiểm soát tốc độ đánh
    private float nextAttackTime = 0f;

    void Update()
    {
        // isPressed cho phép GIỮ chuột để xả đạn (hoặc chém) liên tục
        if (Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            // Kiểm tra xem đã hết thời gian hồi chiêu chưa
            if (Time.time >= nextAttackTime)
            {
                Attack();
            }
        }
    }

    void Attack()
    {
        // 1. Tính toán vị trí chuột và góc xoay (dùng chung cho cả 3 vũ khí)
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        mouseWorldPos.z = 0f;

        Vector2 lookDir = mouseWorldPos - transform.position;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;

        // 2. Lấy thông tin vũ khí đang được chọn từ Menu
        MainMenuManager.WeaponType currentWeapon = MainMenuManager.selectedWeapon;

        // 3. Thực thi đòn đánh tương ứng
        if (currentWeapon == MainMenuManager.WeaponType.Pistol)
        {
            ShootPistol(lookDir, angle);
            nextAttackTime = Time.time + pistolCooldown; // Thiết lập thời gian chờ cho viên tiếp theo
        }
        else if (currentWeapon == MainMenuManager.WeaponType.Shotgun)
        {
            ShootShotgun(lookDir, angle);
            nextAttackTime = Time.time + shotgunCooldown;
        }
        else if (currentWeapon == MainMenuManager.WeaponType.Sword)
        {
            SwingSword(lookDir, angle);
            nextAttackTime = Time.time + swordCooldown;
        }
    }

    void ShootPistol(Vector2 dir, float angle)
    {
        // Bắn 1 viên thẳng tắp
        GameObject bullet = Instantiate(pistolBulletPrefab, transform.position, Quaternion.Euler(0, 0, angle));
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        rb.AddForce(dir.normalized * pistolForce, ForceMode2D.Impulse);
    }

    void ShootShotgun(Vector2 dir, float baseAngle)
    {
        // Tính toán để rải đều các viên đạn chùm trong phạm vi góc spreadAngle
        float angleStep = spreadAngle / (pelletsCount - 1);
        float startAngle = baseAngle - (spreadAngle / 2f);

        for (int i = 0; i < pelletsCount; i++)
        {
            // Tính góc xoay cho từng viên đạn nhỏ
            float pelletAngle = startAngle + (angleStep * i);
            GameObject pellet = Instantiate(shotgunBulletPrefab, transform.position, Quaternion.Euler(0, 0, pelletAngle));

            // Tính lại hướng bay (Vector2) từ góc xoay mới
            Vector2 pelletDir = new Vector2(Mathf.Cos(pelletAngle * Mathf.Deg2Rad), Mathf.Sin(pelletAngle * Mathf.Deg2Rad));

            Rigidbody2D rb = pellet.GetComponent<Rigidbody2D>();
            rb.AddForce(pelletDir.normalized * shotgunForce, ForceMode2D.Impulse);
        }
    }

    void SwingSword(Vector2 dir, float angle)
    {
        // Tính toán vị trí xuất hiện của nhát chém (nằm ngay trước mặt nhân vật)
        Vector3 spawnPos = transform.position + (Vector3)(dir.normalized * swordRange);

        // Tạo ra nhát chém
        GameObject slash = Instantiate(swordSlashPrefab, spawnPos, Quaternion.Euler(0, 0, angle));

        // MẸO: Bạn có thể đẩy nhẹ nhân vật lên phía trước một chút mỗi khi vung kiếm để tạo "lực"
        // GetComponent<Rigidbody2D>().AddForce(dir.normalized * 3f, ForceMode2D.Impulse); 
    }
}