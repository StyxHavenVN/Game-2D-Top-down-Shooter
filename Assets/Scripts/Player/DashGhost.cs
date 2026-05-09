using UnityEngine;
using System.Collections;

/// <summary>
/// Tạo hiệu ứng bóng mờ (Afterimage) mỗi khi nhân vật Dash.
/// Cực kỳ "cuốn" và đẹp mắt cho game 2D.
/// Chỉ cần kéo thả script này vào cùng chỗ với PlayerDash là xong.
/// </summary>
public class DashGhost : MonoBehaviour
{
    [Header("Cài đặt Bóng mờ")]
    [Tooltip("Khoảng cách thời gian tạo ra một cái bóng mới")]
    public float ghostDelay = 0.05f;
    [Tooltip("Thời gian tồn tại của bóng trước khi tan biến")]
    public float destroyTime = 0.3f;
    [Tooltip("Màu của bóng (Nên để hơi trong suốt)")]
    public Color ghostColor = new Color(0.5f, 0.8f, 1f, 0.6f);

    private PlayerDash playerDash;
    private SpriteRenderer sr;
    private float ghostTimer;

    void Start()
    {
        playerDash = GetComponent<PlayerDash>();
        // Tự động tìm SpriteRenderer trên nhân vật
        sr = GetComponentInChildren<SpriteRenderer>();
    }

    void Update()
    {
        // Liên tục tạo bóng nếu nhân vật đang trong trạng thái Dash
        if (playerDash != null && playerDash.IsDashing() && sr != null)
        {
            if (ghostTimer > 0)
            {
                ghostTimer -= Time.deltaTime;
            }
            else
            {
                CreateGhost();
                ghostTimer = ghostDelay;
            }
        }
    }

    void CreateGhost()
    {
        // Tạo một GameObject tạm thời để chứa cái bóng
        GameObject ghostObj = new GameObject("DashGhost");
        ghostObj.transform.position = sr.transform.position;
        ghostObj.transform.rotation = sr.transform.rotation;
        ghostObj.transform.localScale = sr.transform.localScale;

        // Copy hình ảnh hiện tại của nhân vật
        SpriteRenderer ghostSr = ghostObj.AddComponent<SpriteRenderer>();
        ghostSr.sprite = sr.sprite;
        ghostSr.color = ghostColor;
        ghostSr.sortingLayerID = sr.sortingLayerID;
        ghostSr.sortingOrder = sr.sortingOrder - 1; // Nằm dưới nhân vật chính

        // Bắt đầu làm mờ và xoá
        StartCoroutine(FadeOut(ghostSr));
    }

    IEnumerator FadeOut(SpriteRenderer ghostSr)
    {
        float t = 0;
        Color startColor = ghostSr.color;
        
        while (t < destroyTime)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(startColor.a, 0f, t / destroyTime);
            ghostSr.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }
        
        Destroy(ghostSr.gameObject);
    }
}
