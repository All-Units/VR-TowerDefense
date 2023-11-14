using UnityEngine;

public class BubbleMenuManager : MonoBehaviour
{
    [SerializeField] private BubbleMenuController menuPrefab;
    [SerializeField] private GameObject camPrefab;
    private BubbleMenuController _menuController;
    private GameObject _camera;

    private void Awake()
    {
        _menuController = Instantiate(menuPrefab);
        _camera = Instantiate(camPrefab);

        _menuController.towerCamera = _camera;
        _menuController._Hide();
    }
}
