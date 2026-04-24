using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerExperience : MonoBehaviour
{
    // Tạo một Singleton để Quái vật ở khắp nơi có thể dễ dàng gửi XP về cho Player
    public static PlayerExperience instance;

    [Header("Chỉ số Kinh nghiệm")]
    public int currentLevel = 1;
    public int currentXP = 0;
    public int xpToNextLevel = 100; // XP cần để lên cấp 2

    [Header("Giao diện UI")]
    public TextMeshProUGUI levelText;
    public Image xpFillImage;
    public TextMeshProUGUI xpText;

    void Awake()
    {
        if (instance == null) instance = this;
    }

    void Start()
    {
        UpdateXPUI();
    }

    // Hàm này sẽ được gọi khi giết quái
    public void AddXP(int amount)
    {
        currentXP += amount;

        // Dùng vòng lặp while: Đề phòng trường hợp giết Boss nhận quá nhiều XP, được lên 2-3 cấp cùng lúc
        while (currentXP >= xpToNextLevel)
        {
            LevelUp();
        }

        UpdateXPUI();
    }

    void LevelUp()
    {
        currentXP -= xpToNextLevel; // Giữ lại phần XP bị dư để đập vào cấp mới
        currentLevel++;

        // Tăng độ khó: Cấp tiếp theo sẽ cần nhiều XP hơn gấp rưỡi (1.5 lần)
        xpToNextLevel = Mathf.RoundToInt(xpToNextLevel * 1.5f);

        Debug.Log("LÊN CẤP! Đạt Cấp độ: " + currentLevel);

        // MẸO: Bạn có thể gọi hàm hồi đầy máu cho Player ở ngay đây!
    }

    void UpdateXPUI()
    {
        // 1. Cập nhật số Level to đùng
        if (levelText != null) levelText.text = currentLevel.ToString();

        // 2. Cập nhật thanh trượt
        if (xpFillImage != null)
        {
            xpFillImage.fillAmount = (float)currentXP / xpToNextLevel;
        }

        // 3. Cập nhật dòng chữ chi tiết
        if (xpText != null)
        {
            int percent = Mathf.RoundToInt(((float)currentXP / xpToNextLevel) * 100);
            // Hàm "N0" giúp tự động phẩy hàng nghìn cho số lớn (VD: 1,000 thay vì 1000)
            xpText.text = currentXP.ToString("N0") + " / " + xpToNextLevel.ToString("N0") + " XP (" + percent + "%)";
        }
    }
}