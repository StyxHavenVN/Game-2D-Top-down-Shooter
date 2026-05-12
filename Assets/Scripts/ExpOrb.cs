using UnityEngine;

public class ExpOrb : MonoBehaviour
{
    [Header("Giá trị kinh nghiệm")]
    public int expValue = 10; 

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerStats stats = other.GetComponent<PlayerStats>();
            if (stats != null)
            {
                stats.AddExp(expValue); 
                Destroy(gameObject);  
            }
        }
    }
}