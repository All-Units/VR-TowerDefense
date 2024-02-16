using System;
using TMPro;
using UnityEngine;

public class BubbleMenuOption : MonoBehaviour
{
    [SerializeField] private TMP_Text title;
    
    private BubbleMenuController _controller;
    private TowerUpgrade _upgrade;

    private Action _callback;
    private int cost = -1;
    
    [SerializeField] private Color cantAffordTextColor = Color.red;
    private Color currentTextColor;

    private void OnDestroy()
    {
        
    }

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

    private void CanAfford(int cash)
    {
        title.color = cash < cost ? cantAffordTextColor : Color.white;
    }
    
    public void Initialize(Action ctx, string displayText, int cost)
    {
        Initialize(ctx, $"${cost}\n {displayText}");
        this.cost = cost;
        CanAfford(CurrencyManager.CurrentCash);
        CurrencyManager.OnChangeMoneyAmount += CanAfford;
    }

    public void PerformOption()
    {
        _callback?.Invoke();
    }

    private readonly Color _grey = new Color(.4f, .4f, .4f, 0);
    public static bool IsCurrentlyHovering => _currentHoveredBubble != null;
    static BubbleMenuOption _currentHoveredBubble = null;
    public void OnHoverStart()
    {
        title.color -= _grey;
        _currentHoveredBubble = this;
        XRControllerTowerController.DeselectCurrent();
    }

    public void OnHoverEnd()
    {
        _currentHoveredBubble = null;
        CanAfford(CurrencyManager.CurrentCash);
    }
    
    public void Upgrade()
    {
        _controller.Upgrade(_upgrade);
    }

    public void Disable()
    {
        
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}