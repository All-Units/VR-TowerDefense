using System;
using UnityEngine;
using UnityEngine.Serialization;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager instance;
    [FormerlySerializedAs("StartingMoney")] public int startingMoney = 10; //Todo Refactor to level dto
    public static int CurrentCash => instance != null ? instance.currentMoney : -1;

    private int currentMoney 
    {
        get => _cash;
        set
        {
            _cash = value;
            OnChangeMoneyAmount?.Invoke(_cash);
        }
    }
    /// <summary>
    /// On the player's cash value changing. Invoked with the new total cash value
    /// </summary>
    public static event Action<int> OnChangeMoneyAmount;

    private int _cash;
    private void Awake()
    {
        currentMoney = startingMoney;
        instance = this;
    }

    public static bool CanAfford(int amt)
    {
        if (instance == null) return false;
        return (instance.currentMoney >= amt);
    }

    public static void SetPlayerMoney(int amt)
    {
        if (instance)
            instance.currentMoney = amt;
    }
    public static void GiveToPlayer(int amt)
    {
        if (instance)
            instance.currentMoney += amt;
    }

    public static void TakeFromPlayer(int amt)
    {
        if (instance)
            instance.currentMoney -= amt;
    }
}
