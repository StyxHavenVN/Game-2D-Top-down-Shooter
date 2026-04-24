using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("UI & Object References")]
    public GameObject weaponSelectionUI; // Kéo thả Panel chọn vũ khí vào đây
    public GameObject player;            // Kéo thả nhân vật Player vào đây

    // Enum giúp định nghĩa các loại vũ khí gọn gàng hơn
    public enum WeaponType { Pistol, Shotgun, Sword }

    // Biến static để các script khác (như PlayerAttack) có thể dễ dàng đọc được vũ khí đang cầm
    public static WeaponType currentWeapon;

    void Start()
    {
        // Khi mới vào game: Dừng thời gian, hiện bảng chọn vũ khí, ẩn Player đi
        Time.timeScale = 0f;
        weaponSelectionUI.SetActive(true);
        player.SetActive(false);
    }

    // 3 Hàm này sẽ được gọi khi bấm 3 nút tương ứng
    public void SelectPistol()
    {
        currentWeapon = WeaponType.Pistol;
        StartGame();
    }

    public void SelectShotgun()
    {
        currentWeapon = WeaponType.Shotgun;
        StartGame();
    }

    public void SelectSword()
    {
        currentWeapon = WeaponType.Sword;
        StartGame();
    }

    private void StartGame()
    {
        // Tắt bảng chọn, hiện Player, và cho thời gian chạy lại bình thường
        weaponSelectionUI.SetActive(false);
        player.SetActive(true);
        Time.timeScale = 1f;

        // In ra Console để test xem đã lưu đúng vũ khí chưa
        Debug.Log("Game Bắt Đầu! Vũ khí đang trang bị: " + currentWeapon.ToString());
    }
}