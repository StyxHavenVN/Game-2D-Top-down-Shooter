using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public int level = 1;
    public int currentExp = 0;
    public int expToNextLevel = 100;

    // Biến này để gọi script Health, giúp hồi máu khi lên cấp
    private Health playerHealth;

    void Start()
    {
        playerHealth = GetComponent<Health>();
    }

    // Hàm này sẽ được gọi khi quái chết
    public void AddExp(int expAmount)
    {
        currentExp += expAmount;
        Debug.Log($"Nhận được {expAmount} EXP! Hiện tại: {currentExp}/{expToNextLevel}");

        // Kiểm tra xem đủ điểm lên cấp chưa
        if (currentExp >= expToNextLevel)
        {
            LevelUp();
        }
    }

    void LevelUp()
    {
        level++;
        currentExp -= expToNextLevel; // Trừ đi số exp đã dùng để lên cấp
        expToNextLevel = Mathf.RoundToInt(expToNextLevel * 1.5f); // Cấp sau cần nhiều exp hơn cấp trước 50%

        Debug.Log("LEVEL UP! Chúc mừng bạn đạt cấp " + level);

        // Tăng sức mạnh khi lên cấp (Ví dụ: Tăng máu tối đa và hồi đầy máu)
        if (playerHealth != null)
        {
            playerHealth.maxHealth += 20; // Tăng 20 máu tối đa
            // Nếu bạn muốn hồi đầy máu khi lên cấp, ta sẽ gọi một hàm bên script Health (sẽ thêm ở bước sau)
        }
    }
}