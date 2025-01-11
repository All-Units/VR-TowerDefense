using System;
using System.Collections;
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
    
    [SerializeField] private Color cannotAffordTextColor = Color.red;
    [SerializeField] private Color canAffordTextColor = Color.green;
    private Color currentTextColor;
    XRSimpleInteractable interactable;

    [SerializeField] GameObject level2GO;
    [SerializeField] GameObject level3GO;
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
        if (level2GO && level3GO)
        {
            level2GO.SetActive(false);
            level3GO.SetActive(false);
            if (upgrade.upgrade.GetUpgrades().Count > 0)
            {
                level2GO.SetActive(true);
            }
            else
                level3GO.SetActive(true);
        }
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
        if (IsUpgrade == false && IsTower == false) return;
        Color c = cash < cost ? cannotAffordTextColor : canAffordTextColor;
        string cashTotal = $"${cost}".ColorString(c);
        if (IsUpgrade)
            title.text = $"{cashTotal} - {baseDisplayText}";
        if (IsTower)
            title.text = $"{cashTotal}\n{baseDisplayText}";
        //title.color = cash < cost ? cantAffordTextColor : Color.white;
    }

    [HideInInspector] public Tower_SO _upgradeDTO = null;
    string baseDisplayText = "";
    public bool IsUpgrade = false;
    public bool IsTower = false;
    public void Initialize(Action ctx, string displayText, int cost, string description = "")
    {
        Initialize(ctx, $"{displayText}", description);
        this.cost = cost;
        baseDisplayText = displayText;
        CanAfford(CurrencyManager.CurrentCash);
        CurrencyManager.OnChangeMoneyAmount += CanAfford;
        if (descriptionText == null) return;
        descriptionText.text = description;
        descriptionText.gameObject.SetActive(false);

        StartCoroutine(_delayEnableUpgradeGO());

    }
    IEnumerator _delayEnableUpgradeGO()
    {
        yield return null;
        yield return null;
        if (level2GO && level3GO && _upgradeDTO)
        {
            level2GO.SetActive(false);
            level3GO.SetActive(false);
            if (_upgradeDTO.GetUpgrades().Count > 0)
            {
                level2GO.SetActive(true);
            }
            else
                level3GO.SetActive(true);
        }
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
        //title.color -= _grey;
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