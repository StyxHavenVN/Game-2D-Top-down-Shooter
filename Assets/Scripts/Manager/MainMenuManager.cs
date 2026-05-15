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
    public GameObject healthBarUI;
    public GameObject expBarUI;
    public GameObject killCounterUI;
    public GameObject ammoUI;

    [Header("Game Systems")]
    public EnemySpawner enemySpawner;

    public enum WeaponType
    {
        Pistol,
        Shotgun,
        Sword,
        None
    }

    public static WeaponType selectedWeapon = WeaponType.None;

    void Start()
    {
        Time.timeScale = 0f;

        selectedWeapon = WeaponType.None;

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);

        if (player != null)
            player.SetActive(false);

        if (healthBarUI != null)
            healthBarUI.SetActive(false);

        if (expBarUI != null)
            expBarUI.SetActive(false);

        if (killCounterUI != null)
            killCounterUI.SetActive(false);

        if (startButton != null)
            startButton.SetActive(false);

        if (weaponNameText != null)
            weaponNameText.text = "NO WEAPON SELECTED";

        if (weaponStatsText != null)
            weaponStatsText.text = "Please select a weapon to view stats and start playing.";

        if (enemySpawner == null)
            enemySpawner = FindAnyObjectByType<EnemySpawner>();

        if (enemySpawner != null)
            enemySpawner.enabled = false;

        EnemyBullet[] enemyBullets = FindObjectsByType<EnemyBullet>(FindObjectsSortMode.None);
        foreach (EnemyBullet bullet in enemyBullets)
        {
            Destroy(bullet.gameObject);
        }
    }

    public void SelectPistol()
    {
        selectedWeapon = WeaponType.Pistol;

        if (weaponNameText != null)
            weaponNameText.text = "PISTOL";

        if (weaponStatsText != null)
            weaponStatsText.text = "Damage: Low\nFire rate: Fast\nAmmo: 17";

        if (startButton != null)
            startButton.SetActive(true);
    }

    public void SelectShotgun()
    {
        selectedWeapon = WeaponType.Shotgun;

        if (weaponNameText != null)
            weaponNameText.text = "SHOTGUN";

        if (weaponStatsText != null)
            weaponStatsText.text = "Damage: High\nFire rate: Slow\nAmmo: 5\nShape: Cone blast";

        if (startButton != null)
            startButton.SetActive(true);
    }

    public void SelectSword()
    {
        selectedWeapon = WeaponType.Sword;

        if (weaponNameText != null)
            weaponNameText.text = "MELEE SWORD";

        if (weaponStatsText != null)
            weaponStatsText.text = "Damage: Very high\nRange: Melee\nEffect: Lifesteal";

        if (startButton != null)
            startButton.SetActive(true);
    }

    public void ClickStartGame()
    {
        if (selectedWeapon == WeaponType.None)
        {
            Debug.LogWarning("[MainMenuManager] Chưa chọn vũ khí.");
            return;
        }

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);

        if (player != null)
            player.SetActive(true);

        if (healthBarUI != null)
            healthBarUI.SetActive(true);

        if (expBarUI != null)
            expBarUI.SetActive(true);

        if (killCounterUI != null)
            killCounterUI.SetActive(true);

        if (ammoUI != null)
            ammoUI.SetActive(true);

        // Bật game chạy lại
        Time.timeScale = 1f;

        // Bật spawner sau khi bắt đầu game
        if (enemySpawner != null)
            enemySpawner.enabled = true;

        Debug.Log("[MainMenuManager] Game started with weapon: " + selectedWeapon);
    }
}
