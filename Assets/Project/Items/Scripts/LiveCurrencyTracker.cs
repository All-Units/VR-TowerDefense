using System;
using System.Collections;
using System.Collections.Generic;
using Project.Towers.Scripts;
using TMPro;
using UnityEngine;

public class LiveCurrencyTracker : MonoBehaviour
{
    [SerializeField] private string displayString = "CURRENT: [GOLD] gp";
    [SerializeField] private TextMeshProUGUI currencyText;
    // Start is called before the first frame update
    void Start()
    {
        CurrencyManager.OnChangeMoneyAmount.AddListener(_RefreshCurrencyText);
        CurrencyManager.instance.StartCoroutine(waitThenRefresh(this));
        TowerSpawnManager.OnTowerSet.AddListener(_RefreshCurrencyText);
    }

    
    static IEnumerator waitThenRefresh(LiveCurrencyTracker tracker)
    {
        yield return new WaitForSeconds(0.1f);
        tracker._RefreshCurrencyText();
    }

    private void _RefreshCurrencyText()
    {
        string s = displayString.Replace("[GOLD]",CurrencyManager.CurrentCashString);
        var towerSO = TowerSpawnManager.GetCurrentTower;
        
        if (towerSO != null && s.Contains("TOWERCOST"))
        {
            string color = CurrencyManager.CanAfford(towerSO) ? "green" : "red";
            string coloredCost = $"<color={color}>COST: {towerSO.cost} gp</color>";
            s = s.Replace("[TOWERCOST]", coloredCost);
        }
       
        currencyText.text = s;
    }
}
