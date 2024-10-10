using UnityEngine;

[CreateAssetMenu(menuName = "SO/Status Modifier", fileName = "New Status Modifier")]
public class StatusModifier : ScriptableObject
{
    public StatusEffectType effectType;
    public int BurnScalar = 1;
    public Color BurnColor = Color.blue;
    public Color AltBurnColor = Color.white;
    public void ApplyStatus(StatusEffectController controller)
    {
        //controller.BurnColor = BurnColor;
        //controller.AltBurnColor = AltBurnColor;

        if (controller.SetBurn(this) == false)
            controller.ApplyStatus(effectType, BurnScalar);
    }
}