using UnityEngine;

public class ExpOrb : MonoBehaviour
{
    [Header("Giá trị kinh nghiệm")]
    public int expValue = 10; // Cục này cho bao nhiêu điểm?

    // Hàm này tự động chạy khi có một vật thể chạm vào cục EXP
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Kiểm tra xem người chạm vào có dán nhãn là "Player" hay không
        if (other.CompareTag("Player"))
        {
            // Lấy script PlayerStats trên người Player
            PlayerStats stats = other.GetComponent<PlayerStats>();
            if (stats != null)
            {
                stats.AddExp(expValue); // Cộng điểm!
                Destroy(gameObject);    // Bị ăn rồi thì biến mất
            }
        }
    }
}