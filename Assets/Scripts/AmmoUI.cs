using UnityEngine;
using TMPro;

public class AmmoUI : MonoBehaviour
{
    public PlayerAttack playerAttack;
    public TextMeshProUGUI ammoText;
    public GameObject menuPanel;

    void Start()
    {
        if (playerAttack == null)
        {
            playerAttack = FindAnyObjectByType<PlayerAttack>();
        }

        if (ammoText == null)
        {
            ammoText = GetComponent<TextMeshProUGUI>();
        }

        HideAmmo();
    }

    void Update()
    {
        if (ammoText == null) return;

        if (playerAttack == null)
        {
            playerAttack = FindAnyObjectByType<PlayerAttack>();
        }

        // Menu còn hiện thì ẩn số đạn
        if (menuPanel != null && menuPanel.activeSelf)
        {
            HideAmmo();
            return;
        }

        // Menu tắt rồi thì hiện số đạn
        ShowAmmo();

        if (playerAttack == null) return;

        MainMenuManager.WeaponType currentWeapon = MainMenuManager.selectedWeapon;

        if (playerAttack.IsReloading())
        {
            ammoText.text = "Reloading...";
            return;
        }

        if (currentWeapon == MainMenuManager.WeaponType.Pistol)
        {
            ammoText.text = playerAttack.GetPistolCurrentAmmo() + " / " + playerAttack.GetPistolMaxAmmo();
        }
        else if (currentWeapon == MainMenuManager.WeaponType.Shotgun)
        {
            ammoText.text = playerAttack.GetShotgunCurrentAmmo() + " / " + playerAttack.GetShotgunMaxAmmo();
        }
        else if (currentWeapon == MainMenuManager.WeaponType.Sword)
        {
            ammoText.text = "SWORD";
        }
        else
        {
            ammoText.text = "";
        }
    }

    void HideAmmo()
    {
        if (ammoText != null)
        {
            ammoText.enabled = false;
            ammoText.text = "";
        }
    }

    void ShowAmmo()
    {
        if (ammoText != null)
        {
            ammoText.enabled = true;
        }
    }
}