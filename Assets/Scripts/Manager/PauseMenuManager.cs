using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenuManager : MonoBehaviour
{
    [Header("Pause UI")]
    public GameObject pausePanel;
    public GameObject optionsPanel;

    [Header("Menu chọn vũ khí")]
    public GameObject mainMenuPanel;

    [Header("Game UI")]
    public GameObject player;
    public GameObject healthBarUI;
    public GameObject expBarUI;
    public GameObject killCounterUI;
    public GameObject ammoUI;

    [Header("Game Systems")]
    public EnemySpawner enemySpawner;

    private bool isPaused = false;

    void Start()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (optionsPanel != null)
            optionsPanel.SetActive(false);
    }

    void Update()
    {
        if (Keyboard.current == null) return;

        // Nếu đang ở menu chọn vũ khí thì không cho mở pause
        if (mainMenuPanel != null && mainMenuPanel.activeSelf)
        {
            return;
        }

        // Nếu chưa chọn vũ khí thì cũng không cho pause
        if (MainMenuManager.selectedWeapon == MainMenuManager.WeaponType.None)
        {
            return;
        }

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        // Chặn pause khi đang ở menu chọn vũ khí
        if (mainMenuPanel != null && mainMenuPanel.activeSelf)
            return;

        if (MainMenuManager.selectedWeapon == MainMenuManager.WeaponType.None)
            return;

        isPaused = true;

        if (pausePanel != null)
            pausePanel.SetActive(true);

        if (optionsPanel != null)
            optionsPanel.SetActive(false);

        Time.timeScale = 0f;

        Debug.Log("[PauseMenuManager] Pause");
    }

    public void ResumeGame()
    {
        isPaused = false;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (optionsPanel != null)
            optionsPanel.SetActive(false);

        Time.timeScale = 1f;

        Debug.Log("[PauseMenuManager] Resume");
    }

    public void OpenOptions()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (optionsPanel != null)
            optionsPanel.SetActive(true);

        Debug.Log("[PauseMenuManager] Open Options");
    }

    public void CloseOptions()
    {
        if (optionsPanel != null)
            optionsPanel.SetActive(false);

        if (pausePanel != null)
            pausePanel.SetActive(true);
    }

    public void BackToMenu()
    {
        isPaused = false;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (optionsPanel != null)
            optionsPanel.SetActive(false);

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

        if (ammoUI != null)
            ammoUI.SetActive(false);

        if (enemySpawner != null)
            enemySpawner.enabled = false;

        MainMenuManager.selectedWeapon = MainMenuManager.WeaponType.None;

        Time.timeScale = 0f;

        Debug.Log("[PauseMenuManager] Back To Menu");
    }
}