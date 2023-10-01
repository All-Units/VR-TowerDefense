using System;
using UnityEngine;

public class BubbleMenuOption : MonoBehaviour
{
    private BubbleMenuController _controller;
    private TowerUpgrade _upgrade;

    private Action _callback;

    public void InitializeUpgrade(BubbleMenuController controller, TowerUpgrade upgrade)
    {
        gameObject.SetActive(true);

        _controller = controller;
        _upgrade = upgrade;
    }

    public void Initialize(Action ctx)
    {
        gameObject.SetActive(true);

        _callback = ctx;
    }

    public void PerformOption()
    {
        _callback?.Invoke();
    }
    
    public void OnHoverStart()
    {
        // Debug.Log("On Hover Start");
    }

    public void OnHoverEnd()
    {
        // Debug.Log("On Hover Exit");
    }
    
    public void Upgrade()
    {
        _controller.Upgrade(_upgrade);
    }

    public void Disable()
    {
        gameObject.SetActive(false);
    }
}