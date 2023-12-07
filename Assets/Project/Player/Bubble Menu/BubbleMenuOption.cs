using System;
using TMPro;
using UnityEngine;

public class BubbleMenuOption : MonoBehaviour
{
    [SerializeField] private TMP_Text title;
    
    private BubbleMenuController _controller;
    private TowerUpgrade _upgrade;

    private Action _callback;

    public void InitializeUpgrade(BubbleMenuController controller, TowerUpgrade upgrade)
    {
        gameObject.SetActive(true);

        _controller = controller;
        _upgrade = upgrade;
    }

    public void Initialize(Action ctx, string displayText)
    {
        gameObject.SetActive(true);
        title.text = displayText;

        _callback = ctx;
    }

    public void PerformOption()
    {
        _callback?.Invoke();
    }
    
    public void OnHoverStart()
    {
        // Debug.Log("On Hover Start");
        // title.gameObject.SetActive(true)
        title.color = Color.gray;
    }

    public void OnHoverEnd()
    {
        // Debug.Log("On Hover Exit");
        // title.gameObject.SetActive(false);
        title.color = Color.white;
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