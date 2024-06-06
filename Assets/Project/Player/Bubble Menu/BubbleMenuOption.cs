using System;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class BubbleMenuOption : MonoBehaviour
{
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text descriptionText;
    
    private BubbleMenuController _controller;
    private TowerUpgrade _upgrade;

    private Action _callback;
    private int cost = -1;
    
    [SerializeField] private Color cantAffordTextColor = Color.red;
    private Color currentTextColor;
    XRSimpleInteractable interactable;
    private void Awake()
    {
        interactable = GetComponent<XRSimpleInteractable>();
        interactable.firstHoverEntered.AddListener(_FirstHoverEntered);
        interactable.lastHoverExited.AddListener(_LastHoverExited);
    }

    private void OnDestroy()
    {
        
    }

    public void InitializeUpgrade(BubbleMenuController controller, TowerUpgrade upgrade)
    {
        gameObject.SetActive(true);

        _controller = controller;
        _upgrade = upgrade;
    }

    public void Initialize(Action ctx, string displayText, string description = "")
    {
        gameObject.SetActive(true);
        title.text = displayText;
        if (descriptionText != null)
        {
            descriptionText.text = description;
            descriptionText.gameObject.SetActive(false);
        }
        //print($"Initialized OPTION: {gameObject.name} to : {displayText}");
        _callback = ctx;
    }

    private void CanAfford(int cash)
    {
        title.color = cash < cost ? cantAffordTextColor : Color.white;
    }

    [HideInInspector] public Tower_SO _upgradeDTO = null;
    
    public void Initialize(Action ctx, string displayText, int cost, string description = "")
    {
        Initialize(ctx, $"${cost}\n {displayText}", description);
        this.cost = cost;
        CanAfford(CurrencyManager.CurrentCash);
        CurrencyManager.OnChangeMoneyAmount += CanAfford;
        if (descriptionText == null) return;
        descriptionText.text = description;
        descriptionText.gameObject.SetActive(false);
    }

    public void PerformOption()
    {
        if (_upgradeDTO != null)
        {
            _callback = () => BubbleMenuController.Upgrade(_upgradeDTO);
        }
        _callback?.Invoke();
        _upgradeDTO = null;
        
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
    void _FirstHoverEntered(HoverEnterEventArgs a)
    {
        if (descriptionText == null) return;
        descriptionText.gameObject.SetActive(true);
    }
    void _LastHoverExited(HoverExitEventArgs a)
    {
        if (descriptionText == null) return;
        descriptionText.gameObject.SetActive(false);
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