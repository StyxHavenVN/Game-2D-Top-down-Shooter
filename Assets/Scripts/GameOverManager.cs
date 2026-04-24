using UnityEngine;
using UnityEngine.SceneManagement; 

public class GameOverManager : MonoBehaviour
{
    [Header("Giao diện Game Over")]
    public GameObject gameOverPanel;

    void Start()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    // Hàm này sẽ được gọi khi nhân vật hết máu
    public void ShowGameOver()
    {
        gameOverPanel.SetActive(true);
        Time.timeScale = 0f; // Dừng đọng thời gian hoàn toàn
    }

    // Hàm này gắn vào nút bấm "Thử Lại"
    public void RestartGame()
    {
        Time.timeScale = 1f; // Trả lại thời gian trôi bình thường
        // Lệnh này giúp Load lại chính cái màn chơi hiện tại từ đầu
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}