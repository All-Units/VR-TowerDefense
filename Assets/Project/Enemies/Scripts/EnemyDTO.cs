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


    [Header("Movement values")]
    public float MoveSpeed = 5f;
    public float rotateDamping = 1;
    public float targetTolerance = 1;

    public float StuckTimeTolerance = 1f;
    public float StuckAngleOffset = 15f;

}
