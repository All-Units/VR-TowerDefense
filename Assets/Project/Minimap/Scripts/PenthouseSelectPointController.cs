using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit;

public class PenthouseSelectPointController : MonoBehaviour
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private GameObject secondTitlePanel;

    [SerializeField] private XRBaseInteractable table;
    private TextMeshProUGUI secondText;
    public Tower originTower;
    private void Awake()
    {
        
        
        table.activated.AddListener(OnSelectTower);
       
        RefreshText();
        secondTitlePanel.SetActive(false);

    }

    public void RefreshText()
    {
        if (secondText == null)
            secondText = secondTitlePanel.GetComponentInChildren<TextMeshProUGUI>();
        //secondText.text = towerDTO.name;
    }

   
    public void OnSelectTower(ActivateEventArgs args)
    {
        InventoryManager.instance.ReleaseAllItems();
        PlayerStateController.instance.TeleportPlayerToPenthouse();
    }

    
    


    void _SetText(string t)
    {
        titleText.text = t;
    }
}