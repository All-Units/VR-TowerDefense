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
        transform.localScale = Vector3.one * stats.radius * 2;
    }
}
