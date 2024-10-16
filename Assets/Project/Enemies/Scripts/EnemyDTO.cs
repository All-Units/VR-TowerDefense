using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/EnemyDTO", fileName = "New Enemy")]
public class EnemyDTO : ScriptableObject
{
    public EnemyType type;
    [Header("Gameplay values")]
    public int KillValue = 1;
    public int Health = 20;
    [Tooltip("The amount of time a Greg spends as a ragdoll before despawning")]
    public int RagdollTime = 5;
    public float RagdollForce = 300f;
    public float MaxRagdollForce = 300f;
    public float MinRagdollYForce = 5f;


    [Header("Attack variables")]
    /// <summary>
    /// Minimum distance to tower
    /// </summary>
    public int MinRange = 1;
    /// <summary>
    /// Max distance to tower
    /// </summary>
    public int MaxRange = 2;

    public int Damage = 1;
    public float attackThreshold = 1.2f;

    [Header("Power attack variables")]
    public float MinPowerAttackTime = 2f;
    [Tooltip("How long we should wait before checking for another power attack")]
    public float PowerAttackTime = 1f;

    public float PowerAttackChance = 0.1f;

    public float PowerAttackScalar = 1.5f;



    [Header("Movement values")]
    public float MoveSpeed = 5f;
    public float MoveSpeedVariance = 0.4f;
    public float rotateDamping = 1;
    public float targetTolerance = 1;

    public float CheckForNeighborsRate = 1f;

    public float StuckAngleOffset = 15f;

}
