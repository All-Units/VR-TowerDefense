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

    public Tower_SO towerSO;

    private void Start()
    {
        selectedSprite.SetActive(false);
        if (currentlySelected == null)
            Select();
    }

    private static TowerIcon currentlySelected;

    
    public void Select()
    {
        if (currentlySelected != null)
            currentlySelected.OnDeselect();
        currentlySelected = this;
        selectedSprite.SetActive(true);
        TowerSpawnManager.SetTower(towerSO);
    }

    void OnDeselect()
    {
        selectedSprite.SetActive(false);
    }
}
