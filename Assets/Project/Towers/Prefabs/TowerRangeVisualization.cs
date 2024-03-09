using UnityEngine;

[ExecuteAlways]
public class TowerRangeVisualization : MonoBehaviour
{
    [SerializeField] private ProjectileTower_SO stats;
    public void SetStats(ProjectileTower_SO stats)
    {
        this.stats = stats;
        SetScale();
    }
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
