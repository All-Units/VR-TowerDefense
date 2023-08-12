using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerIcon : MonoBehaviour
{
    [SerializeField] private GameObject selectedSprite;

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
    }

    void OnDeselect()
    {
        selectedSprite.SetActive(false);
    }
}
