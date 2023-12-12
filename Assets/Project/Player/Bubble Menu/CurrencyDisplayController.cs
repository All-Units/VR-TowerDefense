using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CurrencyDisplayController : MonoBehaviour
{
    [SerializeField] private TMP_Text text;

    private void Start()
    {
        CurrencyManager.OnChangeMoneyAmount.AddListener(OnCurrencyChange);
    }

    private void OnDestroy()
    {
        CurrencyManager.OnChangeMoneyAmount.RemoveListener(OnCurrencyChange);
    }

    private void OnCurrencyChange(int amt)
    {
        text.text = $"${amt}";
    }
}
