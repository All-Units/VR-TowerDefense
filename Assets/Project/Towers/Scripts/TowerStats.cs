using UnityEngine;

[CreateAssetMenu(menuName = "SO/Tower Stats", fileName = "New Tower Stats")]
public class TowerStats : ScriptableObject
{
    public float radius = 5f;
    public float shotCooldown = 1f;
    public Projectile projectile;
}