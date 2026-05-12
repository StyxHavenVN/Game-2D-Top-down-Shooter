using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    private TMP_Text textMesh;
    private float lifetime = 1f;
    private Vector3 moveDir;

    /// <summary>
    /// Hàm tĩnh hỗ trợ việc tạo Popup ở bất kỳ đâu.
    /// Yêu cầu Prefab phải được lưu ở Assets/Resources/DamagePopup.prefab
    /// </summary>
    public static void Create(Vector3 worldPos, int amount, bool isHeal = false)
    {
        GameObject prefab = Resources.Load<GameObject>("DamagePopup");
        if (prefab == null)
        {
            Debug.LogWarning("Không tìm thấy Prefab 'DamagePopup' trong thư mục Assets/Resources!");
            return;
        }

        // Tạo ra sát thương popup với một độ lệch nhỏ ngẫu nhiên để các số không đè lên nhau hoàn toàn
        Vector3 randomOffset = new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(-0.1f, 0.3f), 0);
        GameObject obj = Instantiate(prefab, worldPos + randomOffset, Quaternion.identity);
        obj.GetComponent<DamagePopup>().Setup(amount, isHeal);
    }

    private void Awake() 
    {
        textMesh = GetComponent<TMP_Text>();
        // Tạo hướng bay lên mượt mà (có độ nảy nhẹ sang hai bên)
        moveDir = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(1.5f, 2.5f), 0f);
    }

    public void Setup(int amount, bool isHeal)
    {
        if (textMesh != null)
        {
            if (isHeal)
            {
                textMesh.text = "+" + amount.ToString();
                textMesh.color = Color.green; // Màu xanh lá khi hồi máu
            }
            else
            {
                textMesh.text = amount.ToString();
                // Sát thương lớn thì màu cam, bình thường màu đỏ
                if (amount >= 30)
                {
                    textMesh.color = new Color(1f, 0.4f, 0f); // Cam đậm
                    textMesh.fontSize += 2f; // To hơn
                }
                else
                {
                    textMesh.color = Color.red; 
                }
            }
        }
    }

    private void Update()
    {
        // Bay lên
        transform.position += moveDir * Time.deltaTime;

        // Giảm thời gian tồn tại
        lifetime -= Time.deltaTime;

        // Hiệu ứng Fade out (Mờ dần) khi sắp biến mất
        if (lifetime < 0.5f && textMesh != null)
        {
            Color c = textMesh.color;
            c.a = lifetime * 2f; // Giảm alpha từ 1 -> 0
            textMesh.color = c;
        }

        if (lifetime <= 0) 
        {
            Destroy(gameObject);
        }
    }
}
