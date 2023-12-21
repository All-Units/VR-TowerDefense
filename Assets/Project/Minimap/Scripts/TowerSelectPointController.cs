using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit;

public class TowerSelectPointController : MonoBehaviour
{
    [SerializeField] public Tower_SO towerDTO;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private GameObject secondTitlePanel;

    [SerializeField] private XRBaseInteractable table;
    private TextMeshProUGUI secondText;
    public PlayerControllableTower originTower;
    private void Awake()
    {
        if(towerDTO != null)
            titleText.text = towerDTO.name;
        table.hoverEntered.AddListener(_HoverEnter);
        table.hoverExited.AddListener(_HoverExit);
        table.activated.AddListener(OnSelectTower);
        table.selectEntered.AddListener(_SelectEntered);
        table.selectExited.AddListener(_SelectExited);
        RefreshText();
        secondTitlePanel.SetActive(false);

    }

    public void RefreshText()
    {
        if (secondText == null)
            secondText = secondTitlePanel.GetComponentInChildren<TextMeshProUGUI>();
        secondText.text = towerDTO.name;
    }

    private void OnValidate()
    {
        if(towerDTO != null && titleText != null)
            titleText.text = towerDTO.name;
    }

    public void OnSelectTower(ActivateEventArgs args)
    {
        if (towerDTO == null)
        {
            Debug.LogError($"No level data assigned to {gameObject.name}", gameObject);
            return;
        }
        PlayerStateController.TakeControlOfTower(originTower);
        //print($"Selected {towerDTO.name}");
    }

    void _HoverEnter(HoverEnterEventArgs args)
    {
        secondTitlePanel.SetActive(true);
    }
    void _HoverExit(HoverExitEventArgs args)
    {
        secondTitlePanel.SetActive(false);
    }

    void _SelectEntered(SelectEnterEventArgs args)
    {
        originTower.healthController.onTakeDamage.AddListener(_TakeDamage);
        _SetHealthText();
    }

    void _TakeDamage(int i)
    {
        _SetHealthText();
    }

    void _SetHealthText()
    {
        string s = $"{originTower.healthController.CurrentHealth} HP";
        _SetText(s);
    }

    void _SelectExited(SelectExitEventArgs args)
    {
        originTower.healthController.onTakeDamage.RemoveListener(_TakeDamage);
    }


    void _SetText(string t)
    {
        if(towerDTO != null && titleText != null)
            titleText.text = t;
    }
}