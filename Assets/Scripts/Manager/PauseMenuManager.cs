using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject pauseMenuPanel;

    [Header("UI Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button quitButton;   

    [Header("Scenes to Load")]
    [SerializeField] private string mainMenuSceneName = "MainMenuScene";
    public static bool IsPauseGame { get; private set; } = false;

    void Start()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        // Nối code với nút bấm
        if (resumeButton != null) resumeButton.onClick.AddListener(ResumeGame);
        if (optionsButton != null) optionsButton.onClick.AddListener(OpenOptions);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(QuitToMainMenu);
        if (quitButton != null) quitButton.onClick.AddListener(QuitGame); 

        Time.timeScale = 1f;
        IsPauseGame = false;
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePauseMenu();
        }

        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
        }
    }

    public void TogglePauseMenu()
    {
        if (pauseMenuPanel == null) return;

        if (IsPauseGame)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    private void PauseGame()
    {
        pauseMenuPanel.SetActive(true);
        Time.timeScale = 0f;
        IsPauseGame = true;
        Debug.Log("Game Paused");
    }

    public void ResumeGame()
    {
        pauseMenuPanel.SetActive(false);
        Time.timeScale = 1f;
        IsPauseGame = false;
        Debug.Log("Game Resumed");
    }

    private void OpenOptions()
    {
        Debug.Log("Opening Options Menu");
    }
    public void QuitToMainMenu()
    {
        Time.timeScale = 1f;
        IsPauseGame = false;

        Debug.Log("Quitting to Main Menu...");
        SceneManager.LoadScene(mainMenuSceneName);
    }
    public void QuitGame()
    {
        Debug.Log("Quitting Game completely...");
        Application.Quit();
    }
}