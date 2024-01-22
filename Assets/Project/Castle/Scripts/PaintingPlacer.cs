using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaintingPlacer : MonoBehaviour
{
    public List<Material> frameColors = new List<Material>();
    public List<Texture> textures = new List<Texture>();
    public MeshRenderer renderer;
    public MeshRenderer frameRenderer;
    public float chance = .5f;
    public void ChangePainting()
    {
        if (renderer == null || textures.Count == 0) return;
        float r = Random.value;
        if (r < chance)
            gameObject.SetActive(true);
        else
            gameObject.SetActive(false);
        frameRenderer.sharedMaterial = frameColors.GetRandom();
        Material mat = new Material(renderer.sharedMaterial);
        mat.SetTexture("_BaseMap", textures.GetRandom());
        renderer.sharedMaterial = mat;
    }
}
