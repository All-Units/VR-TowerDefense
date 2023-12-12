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
        //Todo: Not sure what intended function is for these, but they never unsubscribe from static events. May cause problems -Z
        CurrencyManager.OnChangeMoneyAmount.AddListener(_RefreshCurrencyText);
        CurrencyManager.instance.StartCoroutine(waitThenRefresh(this));
        TowerSpawnManager.OnTowerSet.AddListener(()=>_RefreshCurrencyText(CurrencyManager.CurrentCash));
    }

    
    static IEnumerator waitThenRefresh(LiveCurrencyTracker tracker)
    {
        yield return new WaitForSeconds(0.1f);
        tracker._RefreshCurrencyText(CurrencyManager.CurrentCash);
    }

    private void _RefreshCurrencyText(int amt)
    {
        string s = displayString.Replace("[GOLD]",amt.ToString());
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
