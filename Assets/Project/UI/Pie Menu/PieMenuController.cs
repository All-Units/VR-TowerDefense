using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class PieMenuController : MonoBehaviour
{
    [SerializeField] private GameObject slicePrefab;
    [SerializeField] private Transform sliceRoot;
    [Range(0,1)][SerializeField] private float sliceFillPadding;
    [SerializeField] private float sliceRotationPadding;
    [SerializeField] private List<Sprite> items;
    private List<GameObject> _slices = new();

    [ContextMenu("BuildMenu")]
    public void BuildMenu()
    {
        sliceRoot.DestroyChildrenImmediate();
        _slices.Clear();
        
        foreach (var item in items) 
        {
            _slices.Add(Instantiate(slicePrefab, sliceRoot));
        }

        for (int i = 0; i < _slices.Count; i++)
        {
            var slice = _slices[i];
            slice.GetComponent<Image>().fillAmount = 1f / _slices.Count - sliceFillPadding;
            var slicesCount = i * (360f / _slices.Count);
            slice.transform.rotation = Quaternion.Euler(0,0 , slicesCount - sliceRotationPadding);
        }
    }
}
