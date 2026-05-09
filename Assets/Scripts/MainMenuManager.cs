using UnityEngine;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [Header("Menu UI")]
    public GameObject mainMenuPanel;
    public TextMeshProUGUI weaponNameText;
    public TextMeshProUGUI weaponStatsText;
    public GameObject startButton;

    [Header("Player & Game UI")]
    public GameObject player;
    public GameObject healthBarUI; // MỚI THÊM: Cổng kết nối với thanh máu
    public GameObject expBarUI;
    public GameObject killText;

    public enum WeaponType { Pistol, Shotgun, Sword, None }
    public static WeaponType selectedWeapon = WeaponType.None;

    void Start()
    {
        // Pause time and show menu on start
        Time.timeScale = 0f;
        mainMenuPanel.SetActive(true);
        killText.SetActive(false);
        player.SetActive(false);


        if (healthBarUI != null) healthBarUI.SetActive(false);
        if (expBarUI != null) expBarUI.SetActive(false);

        // Hide Start button and show default text initially
        startButton.SetActive(false);
        weaponNameText.text = "NO WEAPON SELECTED";
        weaponStatsText.text = "Please select a weapon to view stats and start playing.";
    }

    public void SelectPistol()
    {
        selectedWeapon = WeaponType.Pistol;
        weaponNameText.text = "PISTOL";
        weaponStatsText.text = "Damage: Low\nFire rate: Fast\nEffect (5%): Pierce target (12 seconds)";
        startButton.SetActive(true);
    }

    public void SelectShotgun()
    {
        selectedWeapon = WeaponType.Shotgun;
        weaponNameText.text = "SHOTGUN";
        weaponStatsText.text = "Damage: High\nFire rate: Slow (Spread shot)\nEffect (5%): Burn (10 seconds)";
        startButton.SetActive(true);
    }

    public void SelectSword()
    {
        selectedWeapon = WeaponType.Sword;
        weaponNameText.text = "MELEE SWORD";
        weaponStatsText.text = "Damage: Very high\nRange: Melee (High risk)\nEffect (5%): Lifesteal (8 seconds)";
        startButton.SetActive(true);
    }

    public void ClickStartGame()
    {
        // Close menu, enable player, and resume game time
        mainMenuPanel.SetActive(false);
        player.SetActive(true);

        if (healthBarUI != null) healthBarUI.SetActive(true);
        if (expBarUI != null) expBarUI.SetActive(true);

        Time.timeScale = 1f;
        Debug.Log("Game started with weapon: " + selectedWeapon.ToString());
    }
}