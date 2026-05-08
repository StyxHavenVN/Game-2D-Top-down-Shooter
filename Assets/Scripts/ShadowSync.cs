using UnityEngine;

public class ShadowSync : MonoBehaviour
{
    [Header("Kéo vật chủ vào đây")]
    public SpriteRenderer parentRenderer;

    private SpriteRenderer myShadowRenderer;

    void Start()
    {
        myShadowRenderer = GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        if (parentRenderer != null)
        {
            myShadowRenderer.sprite = parentRenderer.sprite;

            myShadowRenderer.flipX = parentRenderer.flipX;
        }
    }
}