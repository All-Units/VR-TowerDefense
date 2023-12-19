using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class MaterialColorChange : MonoBehaviour
{
    private Renderer _renderer;

    [SerializeField] private Color finalColor;
    private Color baseColor;

    public void Start()
    {
        _renderer = GetComponent<Renderer>();
        baseColor = _renderer.material.color;
    }

    public void LerpColor(float t)
    {
        _renderer.material.color = t < 0.01 ? baseColor : Color.Lerp(baseColor, finalColor, Mathf.Clamp01(t));
    }
}