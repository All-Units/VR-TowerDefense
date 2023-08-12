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
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private MeshRenderer _mr;
    [SerializeField] private Material ghostMat;
    private Material[] baseMats;
    private Material[] ghostMats;

    public Tower_SO towerSO;

    private void Start()
    {
        selectedSprite.SetActive(false);
        if (currentlySelected == null)
            Select();

       SetMaterials();
    }

    void SetMaterials()
    {
        baseMats = _mr.sharedMaterials;
        var _ghosts = new List<Material>();
        foreach (Material mat in baseMats)
            _ghosts.Add(ghostMat);
        ghostMats = _ghosts.ToArray();
    }
    private static TowerIcon currentlySelected;

    
    public void Select()
    {
        if (currentlySelected != null)
            currentlySelected.OnDeselect();
        TowerSelectorUI.UpdateAllTowers();
        currentlySelected = this;
        selectedSprite.SetActive(true);
        TowerSpawnManager.SetTower(towerSO);
    }

    void OnDeselect()
    {
        selectedSprite.SetActive(false);
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
