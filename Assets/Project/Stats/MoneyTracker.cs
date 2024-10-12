using JetBrains.Annotations;
using System.Collections;
using UnityEngine;
using System;

[CreateAssetMenu(menuName = "SO/Stats/Money Earned")]
public class MoneyTracker : StatTracker
{
    public int TotalMoneyEarned => total;
    public override string GetDisplayString()
    {
        string num = getSerializeValue.PrettyNumber();
        if (IsInitialized)
            num = total.PrettyNumber();
        return $"{statName}: ${num}";
    }
    protected bool _giveMoneyOnStart = true;
    protected override void InitTracker()
    {
        CurrencyManager.OnChangeMoneyAmount += _OnCurrencyChange;
        //CurrencyManager.instance.startc
        _lastMoneyTotal = CurrencyManager.CurrentCash;
        if (CurrencyManager.CurrentCash > 0 && _giveMoneyOnStart)
        {
            //
            total += CurrencyManager.CurrentCash;
            Debug.Log($"Started wif money, now have {total}");
            InventoryManager.UpdateStats(this);
            _lastMoneyTotal = CurrencyManager.CurrentCash;
            //_lastMoneyTotal = total;
        }


    }
    protected int _lastMoneyTotal = 0;
    protected virtual void _OnCurrencyChange(int newTotal)
    {
        int delta = newTotal - _lastMoneyTotal;
        Debug.Log($"MONEY CHANGED from {_lastMoneyTotal} to {newTotal}, a change of {delta}");
        InventoryManager.UpdateStats(this);
        _lastMoneyTotal = newTotal;
        if (delta <= 0) return;
        total += delta;
        
        InventoryManager.UpdateStats(this);
    }
    
    public override void Print()
    {
        Debug.Log($"Total money: {total}");
    }

    public override void ClearTracker()
    {
        CurrencyManager.OnChangeMoneyAmount -= _OnCurrencyChange;
    }
}
