using UnityEngine;

public class ExpOrb : MonoBehaviour
{
    [Header("Giá trị kinh nghiệm")]
    public int expValue = 10;

    [Header("Tự hủy nếu không nhặt")]
    public float lifeTime = 20f;

    private bool collected = false;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return;

        if (other.CompareTag("Player"))
        {
            PlayerStats stats = other.GetComponentInParent<PlayerStats>();

            if (stats != null)
            {
                collected = true;

                stats.AddExp(expValue);

                // Phát âm thanh nhặt Energy / EXP
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayEnergySound();
                }

                Destroy(gameObject);
            }
            else
            {
                Debug.LogWarning("[ExpOrb] Player không có PlayerStats.");
            }
        }
    }
}