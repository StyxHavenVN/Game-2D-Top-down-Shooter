using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [Header("Kho Vũ Khí (Kéo Script tương ứng ở Player vào đây)")]
    public WeaponBase pistolWeapon;
    public WeaponBase shotgunWeapon;
    public WeaponBase swordWeapon;

    private WeaponBase currentWeaponScript;

    void Start()
    {
        // Ẩn/hiện dựa trên menu (Mặc định nếu quên gán thì thôi)
    }

    void Update()
    {
        // isPressed cho phép GIỮ chuột để xả đạn (hoặc chém) liên tục
        if (Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            Attack();
        }
    }

    void Attack()
    {
        // 1. Cập nhật vũ khí hiện tại dựa theo Menu
        MainMenuManager.WeaponType currentWeapon = MainMenuManager.selectedWeapon;
        
        if (currentWeapon == MainMenuManager.WeaponType.Pistol)
            currentWeaponScript = pistolWeapon;
        else if (currentWeapon == MainMenuManager.WeaponType.Shotgun)
            currentWeaponScript = shotgunWeapon;
        else if (currentWeapon == MainMenuManager.WeaponType.Sword)
            currentWeaponScript = swordWeapon;

        // Nếu chưa chọn vũ khí nào thì thoát
        if (currentWeaponScript == null) return;

        // 2. Kiểm tra xem đã hết thời gian hồi chiêu chưa
        if (currentWeaponScript.CanAttack())
        {
            // Tính toán vị trí chuột và góc xoay
            Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
            mouseWorldPos.z = 0f;

            Vector2 lookDir = mouseWorldPos - transform.position;
            float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;

            // Ra lệnh cho vũ khí thực thi
            currentWeaponScript.Attack(lookDir, angle);
        }
    }
}