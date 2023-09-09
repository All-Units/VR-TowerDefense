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
    public Tower originTower;
    private void Awake()
    {
        if(towerDTO != null)
            titleText.text = towerDTO.name;
        table.hoverEntered.AddListener(_HoverEnter);
        table.hoverExited.AddListener(_HoverExit);
        table.activated.AddListener(OnSelectLevel);
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

    public void OnSelectLevel(ActivateEventArgs args)
    {
        if (towerDTO == null)
        {
            Debug.LogError($"No level data assigned to {gameObject.name}", gameObject);
            return;
        }
        
        PlayerStateController.TakeControlOfTower(originTower);
        print($"Selected {towerDTO.name}");
    }

    void _HoverEnter(HoverEnterEventArgs args)
    {
        secondTitlePanel.SetActive(true);
    }
    void _HoverExit(HoverExitEventArgs args)
    {
        secondTitlePanel.SetActive(false);
    }

    IEnumerator _ResetText()
    {
        yield return new WaitForSeconds(2f);
        _SetText(towerDTO.name);
    }

    void _SetText(string t)
    {
        if(towerDTO != null && titleText != null)
            titleText.text = t;
    }
}