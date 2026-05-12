using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class DropShadow : MonoBehaviour
{
    [Header("Cài đặt Bóng")]
    public Vector2 shadowOffset = new Vector2(0.2f, -0.3f); 
    public Color shadowColor = new Color(0f, 0f, 0f, 0.4f);
    public Vector3 shadowScale = new Vector3(1f, -0.5f, 1f); 
    public float skewAngle = -20f;

    private SpriteRenderer casterRenderer;
    private SpriteRenderer shadowRenderer;
    private Transform shadowTransform;

    void Start()
    {
        casterRenderer = GetComponent<SpriteRenderer>();

        GameObject shadowObject = new GameObject("Shadow_Sprite");
        shadowTransform = shadowObject.transform;
        shadowTransform.parent = transform;
        shadowTransform.localPosition = shadowOffset;
        shadowTransform.localScale = shadowScale;
        shadowTransform.localRotation = Quaternion.Euler(0, 0, skewAngle);
        shadowRenderer = shadowObject.AddComponent<SpriteRenderer>();
        shadowRenderer.sprite = casterRenderer.sprite;
        shadowRenderer.color = shadowColor;
        shadowRenderer.sortingLayerName = casterRenderer.sortingLayerName;
        shadowRenderer.sortingOrder = casterRenderer.sortingOrder - 1;
    }

    void Update()
    {
        if (shadowRenderer.sprite != casterRenderer.sprite)
        {
            shadowRenderer.sprite = casterRenderer.sprite;
        }
    }
}