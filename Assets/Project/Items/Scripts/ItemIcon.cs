using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemIcon : MonoBehaviour
{
    [SerializeField] private GameObject selectedSprite;
    public GameObject iconSprite;

    private static ItemIcon _currentlySelected = null;
    public void OnSelect()
    {
        if (_currentlySelected)
            _currentlySelected.OnDeselect();
        _currentlySelected = this;
        selectedSprite.SetActive(true);
    }

    void OnDeselect()
    {
        selectedSprite.SetActive(false);
    }
}
