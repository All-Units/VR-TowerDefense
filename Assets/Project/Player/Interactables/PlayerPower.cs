using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[CreateAssetMenu(menuName = "SO/Tower Takeover/Player Power", fileName = "New Item")]
public class PlayerPower : TowerTakeoverObject
{
    public XRGrabInteractable throwable;
    public PlayerPowerType type;
}

public enum PlayerPowerType
{
    FIREBALLS,
    FLAMELANCE
}