using JetBrains.Annotations;
using System.Collections;
using UnityEngine;
using System;

[CreateAssetMenu(menuName = "SO/Stats/Money Earned")]
public class MoneyTracker : StatTracker
{
    public int TotalMoneyEarned = 0;
    protected override void InitTracker()
    {
        CurrencyManager.OnChangeMoneyAmount += _OnCurrencyChange;
        
        _lastMoneyTotal = CurrencyManager.CurrentCash;


    }
    int _lastMoneyTotal = 0;
    void _OnCurrencyChange(int total)
    {
        int delta = total - _lastMoneyTotal;
        if (delta <= 0) return;
        TotalMoneyEarned += delta;
        this.total = TotalMoneyEarned;
        _lastMoneyTotal = total;
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