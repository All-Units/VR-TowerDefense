using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager instance;
    public int StartingMoney = 10;
    public int roundBonus = 50;
    public static int CurrentCash => instance.CurrentMoney;
    public static string CurrentCashString => instance.CurrentMoney.ToString();
    public int CurrentMoney {
        get => _cash;
        set
        {
            _cash = value;
            currencyDisplay.text = $"{_cash} gold";
            OnChangeMoneyAmount?.Invoke();
        }
    }

    public static UnityEvent OnChangeMoneyAmount = new UnityEvent();

    private int _cash;
    public TextMeshProUGUI currencyDisplay;
    private void Awake()
    {
        CurrentMoney = StartingMoney;
        instance = this;
        EnemyManager.OnRoundEnded.AddListener(FinishRound);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FinishRound()
    {
        CurrentMoney += roundBonus;
    }

    public static bool CanAfford(Tower_SO tower)
    {
        if (instance == null) return false;
        return (instance.CurrentMoney >= tower.cost);
    }

    public static bool CouldAfford(Tower_SO tower)
    {
        if (instance == null) return false;
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

    public static void PayFor(int cost)
    {
        instance.CurrentMoney -= cost;
    }
    
}
