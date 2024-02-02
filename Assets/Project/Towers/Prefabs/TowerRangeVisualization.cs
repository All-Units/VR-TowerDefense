using UnityEngine;

[ExecuteAlways]
public class TowerRangeVisualization : MonoBehaviour
{
    [SerializeField] private ProjectileTower_SO stats;

    private void Start()
    {
        SetScale();
    }

    [ContextMenu("Set Scale")]
    private void SetScale()
    {
        float r = 1;
        if (stats != null) r = stats.radius;
        transform.localScale = Vector3.one * r * 2;
    }
}
