using UnityEngine;

[CreateAssetMenu(menuName = "SO/Status Modifier", fileName = "New Status Modifier")]
public class StatusModifier : ScriptableObject
{
    public StatusEffectType effectType;
    public void ApplyStatus(StatusEffectController controller)
    {
        controller.ApplyStatus(effectType);
    }
}