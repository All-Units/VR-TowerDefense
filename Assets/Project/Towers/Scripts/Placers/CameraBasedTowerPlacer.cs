using Project.Towers.Scripts;
using UnityEngine;

public class CameraBasedTowerPlacer : MonoBehaviour
{
    [SerializeField] private Camera _camera;
    [SerializeField] private Tower_SO towerToPlace;
    [SerializeField] private LayerMask layerMask;
    private MapTile selectedTile = null;
    private bool _placing = false;
    
    private void Update()
    {
        if(_placing == false) return;

        SelectATile();
    }

    private void SelectATile()
    {
        if (_camera == null) return;
        var ray = _camera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var hit, 1000, layerMask.value))
        {
            var tile = hit.transform.GetComponent<MapTile>();

            if (tile != selectedTile && tile.selectable)
            {
                // TowerSpawnManager.Instance.PlaceGhost(tile.transform.position, transform.position);
                selectedTile = tile;
            }
        }
        else
        {
            selectedTile = null;
            if (TowerSpawnManager.Instance != null)
                TowerSpawnManager.Instance.HideGhost();
        }
    }

    public void OnStartPlacement()
    {
        _placing = true;
    }

    public void OnPlaceTower()
    {
        if(selectedTile)
            TowerSpawnManager.Instance.PlaceTower(selectedTile.transform.position);
        _placing = false;
    }
}