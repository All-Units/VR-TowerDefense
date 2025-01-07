using JetBrains.Annotations;
using System.Collections;
using UnityEngine;
using System;

[CreateAssetMenu(menuName = "SO/Stats/Money Spent")]
public class MoneySpentTracker : MoneyTracker
{
    protected override void InitTracker()
    {
        _giveMoneyOnStart = false;
        _lastMoneyTotal = CurrencyManager.CurrentCash;
        base.InitTracker();
        _lastMoneyTotal = CurrencyManager.CurrentCash;
    }
    protected override void _OnCurrencyChange(int NewCashTotal)
    {
        int delta = NewCashTotal - _lastMoneyTotal;
        //Now do nothing if POSITIVE
        InventoryManager.UpdateStats(this);
        _lastMoneyTotal = NewCashTotal;
        if (delta >= 0) return;
        total += Math.Abs(delta);
        
        InventoryManager.UpdateStats(this);
    }
}