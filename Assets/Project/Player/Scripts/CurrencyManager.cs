using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager instance;
    public int StartingMoney = 10;
    public int CurrentMoney {
        get
        {
            return _cash;
        }
        set
        {
            _cash = value;
            currencyDisplay.text = $"{_cash} gold";
        }
    }

    private int _cash;
    public TextMeshProUGUI currencyDisplay;
    private void Awake()
    {
        CurrentMoney = StartingMoney;
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static bool CanAfford(Tower_SO tower)
    {
        return (instance.CurrentMoney >= tower.cost);
    }

    public static bool CouldAfford(Tower_SO tower)
    {
        return (instance.CurrentMoney - tower.cost >= 0);
    }
    /// <summary>
    /// Checks if the player can afford the tower, and takes out the appropriate cost if true
    /// </summary>
    /// <param name="tower"></param>
    /// <returns></returns>
    public static bool TryCanAfford(Tower_SO tower)
    {
        if (CanAfford(tower))
        {
            instance.CurrentMoney -= tower.cost;
            return true;
        }

        return false;
    }
    
}
