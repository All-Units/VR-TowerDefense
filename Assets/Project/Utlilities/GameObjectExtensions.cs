using UnityEngine;

public static class GameObjectExtensions
{
    public static void SetLayerRecursively(this GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            child.gameObject.SetLayerRecursively(newLayer);
        }
    }
    
    public static void EvenlySpaceChildren(this GameObject obj, float spacing = 1)
    {
        for (int i = 0; i < obj.transform.childCount; i++)
        {
            obj.transform.GetChild(i).localPosition = new Vector3(i * spacing, 0, 0);
        }
    }
    
    public static GameObject OrNull(this GameObject obj)
    {
        return obj != null ? obj : null;
    }
}