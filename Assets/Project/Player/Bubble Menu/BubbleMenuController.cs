using System;
using System.Collections.Generic;
using UnityEngine;

public class BubbleMenuController : MonoBehaviour
{
    private static BubbleMenuController Instance;
    private Tower _currentTower;
    [SerializeField] private GameObject towerCamera;

    private void Awake()
    {
        Instance = this;
        _Hide();
        
        PlayerStateController.OnStateChange += PlayerStateControllerOnOnStateChange;
    }

    private void PlayerStateControllerOnOnStateChange(PlayerState arg1, PlayerState arg2)
    {
        if(arg2 == PlayerState.TOWER_CONTROL)
            _Hide();
    }

    private void Initialize(Tower t)
    {
        _currentTower = t;

        towerCamera.transform.position = t.transform.position;
        towerCamera.gameObject.SetActive(true);
        gameObject.SetActive(true);
        //
        // var main = Camera.main;
        // if (main != null)
        //     transform.position = PlayerStateController.instance.transform.position + main.transform.forward;
    }

    private void ListUpgrades()
    {
        var upgrades = _currentTower.dto.GetUpgrades();
    }

    public static void Open(Tower tower)
    {
        if(Instance == null) return;
        
        Instance.Initialize(tower);
    }

    public static void Hide()
    {
        if (Instance == null) return;
        
        Instance._Hide();
    }

    private void _Hide()
    {
        gameObject.SetActive(false);
    }
}
