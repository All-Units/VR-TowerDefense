using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CashDisplay : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI displayText;
    // Start is called before the first frame update
    void Start()
    {
        if (displayText == null) displayText = GetComponent<TextMeshProUGUI>();
        CurrencyManager.OnChangeMoneyAmount += _OnCurrencyChange;
        _OnCurrencyChange(CurrencyManager.CurrentCash);
    }
    private void OnDestroy()
    {
        CurrencyManager.OnChangeMoneyAmount -= _OnCurrencyChange;   
    }
    void _OnCurrencyChange(int current)
    {
        displayText.text = $"${current}";
    }

    
}
