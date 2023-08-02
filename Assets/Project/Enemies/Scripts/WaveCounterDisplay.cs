using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
public class WaveCounterDisplay : MonoBehaviour
{
    [SerializeField] private GameObject counterPanel;
    public TextMeshProUGUI counter;
    // Start is called before the first frame update
    private void Start()
    {
        SetPanelVisibility(false);
    }

    public void SetPanelVisibility(bool on)
    {
        counterPanel.SetActive(on);
    }

    public void SetText(string s)
    {
        counter.text = s;
    }
}
