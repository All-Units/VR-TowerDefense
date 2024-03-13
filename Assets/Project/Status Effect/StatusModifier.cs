using UnityEngine;

[CreateAssetMenu(menuName = "SO/Status Modifier", fileName = "New Status Modifier")]
public class StatusModifier : ScriptableObject
{
    public StatusEffectType effectType;
    public int BurnScalar = 1;
    public Color BurnColor = Color.blue;
    public void ApplyStatus(StatusEffectController controller)
    {
        controller.BurnColor = BurnColor;
        controller.ApplyStatus(effectType, BurnScalar);
    }
}