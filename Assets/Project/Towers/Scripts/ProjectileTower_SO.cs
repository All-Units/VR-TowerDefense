using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Projectile Tower", fileName = "New Projectile Tower")]
public class ProjectileTower_SO : Tower_SO
{
    [Header("Projectile Data")]
    public float radius = 5f;
    public float shotCooldown = 1f;
    public Projectile projectile;
    public GameObject PlayerProjectile;
    
    public TowerTakeoverObject playerItem_SO;
    
}