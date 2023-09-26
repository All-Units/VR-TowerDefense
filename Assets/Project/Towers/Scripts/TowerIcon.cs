using System;
using System.Collections;
using System.Collections.Generic;
using Project.Towers.Scripts;
using TMPro;
using UnityEngine;

public class TowerIcon : MonoBehaviour
{
    [SerializeField] private GameObject selectedSprite;
    [SerializeField] public TextMeshProUGUI nameText;
    [SerializeField] public TextMeshProUGUI descriptionText;
    [SerializeField] private MeshRenderer _mr;
    [SerializeField] private Material ghostMat;
    [SerializeField] private GameObject displayCanvas;
    private Material[] baseMats;
    private Material[] ghostMats;

    public Tower_SO towerSO;

    private void Start()
    {
        FillText();
        CurrencyManager.OnChangeMoneyAmount.AddListener(_RefreshCanAfford);
    }

    void FillText()
    {
        nameText.text = towerSO.name;
        string color = CurrencyManager.CanAfford(towerSO) ? "green" : "red";
        string coloredCost = $"<color={color}>Cost:{towerSO.cost} gp</color>";
        descriptionText.text = $"{towerSO.description}\n{coloredCost}";
        
    }

    private void _RefreshCanAfford()
    {
        FillText();
    }


    void SetMaterials()
    {
        baseMats = _mr.sharedMaterials;
        var _ghosts = new List<Material>();
        foreach (Material mat in baseMats)
            _ghosts.Add(ghostMat);
        ghostMats = _ghosts.ToArray();
    }
    private static TowerIcon currentlySelected = null;

    
    public void Select()
    {
        if (currentlySelected != null)
            currentlySelected.OnDeselect();
        TowerSelectorItem.UpdateAllTowers();
        currentlySelected = this;
        selectedSprite.SetActive(true);
        TowerSpawnManager.SetTower(towerSO);
        displayCanvas.SetActive(true);
    }

    void OnDeselect()
    {
        selectedSprite.SetActive(false);
        displayCanvas.SetActive(false);
    }

    public void SetCanAfford()
    {
        if (baseMats == null)
            SetMaterials();
        if (CurrencyManager.CouldAfford(towerSO))
        {
            _mr.materials = baseMats;
        }
        else
        {
            _mr.materials = ghostMats;
        }
    }
}
