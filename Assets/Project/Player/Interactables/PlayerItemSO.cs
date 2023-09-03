using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[CreateAssetMenu(menuName = "SO/Player Item SO", fileName = "New Item")]
public class PlayerItemSO : ScriptableObject
{
    public IXRSelectInteractable itemGO;
}
