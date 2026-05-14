using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class DamagePopupSorting : MonoBehaviour
{
    public string sortingLayerName = "Default";
    public int orderInLayer = 100;

    void Awake()
    {
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();

        meshRenderer.sortingLayerName = sortingLayerName;
        meshRenderer.sortingOrder = orderInLayer;
    }
}