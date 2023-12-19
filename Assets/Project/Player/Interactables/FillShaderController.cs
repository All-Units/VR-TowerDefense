using UnityEngine;

public class FillShaderController : MonoBehaviour
{
    private Material material;
    

    [SerializeField] private float min, max;
    private static readonly int FillAmount = Shader.PropertyToID("_FillAmount");

    public void Start()
    {
        material = GetComponent<Renderer>().material;
    }

    public void LerpColor(float t)
    {
        if(material)
            material.SetFloat(FillAmount, Mathf.Lerp(min, max, Mathf.Clamp01(t)));
    }
}