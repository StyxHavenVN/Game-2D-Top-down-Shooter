using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    public TextMeshPro textMesh;

    [Header("Chuyển động")]
    public float moveSpeed = 1.5f;
    public float lifeTime = 0.8f;

    [Header("Scale")]
    public float startScale = 1f;
    public float endScale = 1.4f;

    private float timer;
    private Color textColor;

    void Awake()
    {
        if (textMesh == null)
        {
            textMesh = GetComponent<TextMeshPro>();
        }
    }

    public void Setup(string text, Color color)
    {
        if (textMesh == null) return;

        textMesh.text = text;
        textMesh.color = color;
        textColor = color;

        transform.localScale = Vector3.one * startScale;
    }

    void Update()
    {
        timer += Time.deltaTime;

        transform.position += Vector3.up * moveSpeed * Time.deltaTime;

        float t = timer / lifeTime;

        transform.localScale = Vector3.Lerp(
            Vector3.one * startScale,
            Vector3.one * endScale,
            t
        );

        if (textMesh != null)
        {
            textColor.a = Mathf.Lerp(1f, 0f, t);
            textMesh.color = textColor;
        }

        if (timer >= lifeTime)
        {
            Destroy(gameObject);
        }
    }
}