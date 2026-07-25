using UnityEngine;

public class EmissiveLight : MonoBehaviour
{
    public MeshRenderer MeshRenderer;

    public Color BaseColor = Color.red;

    [Range(0, 1)]
    public float Brightness = 0f; // 0-1
    public float OffIntensity = -3f;
    public float OnIntensity = 1f;

    private MaterialPropertyBlock _mpb;

    private void Start()
    {
        _mpb = new MaterialPropertyBlock();
    }

    private void Update()
    {
        MeshRenderer.GetPropertyBlock(_mpb);
        _mpb.SetColor("_EmissionColor", BaseColor * Mathf.Pow(2f, Mathf.Lerp(OffIntensity, OnIntensity, Brightness)));
        MeshRenderer.SetPropertyBlock(_mpb);
    }
}