using TMPro;
using UnityEngine;

public class CurrencyDisplayController : MonoBehaviour
{
    [SerializeField] private TMP_Text text;

    private void Start()
    {
        CurrencyManager.OnChangeMoneyAmount += OnCurrencyChange;
        OnCurrencyChange(CurrencyManager.CurrentCash);
    }

    private void OnDestroy()
    {
        CurrencyManager.OnChangeMoneyAmount -= OnCurrencyChange;
    }

    private void OnCurrencyChange(int amt)
    {
        text.text = $"${amt}";
    }
}
