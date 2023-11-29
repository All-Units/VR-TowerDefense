using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/EnemyDTO", fileName = "New Enemy")]
public class EnemyDTO : ScriptableObject
{
    public EnemyType type;
    public int KillValue = 1;
    public int MinRange = 1;
    public int MaxRange = 2;
    public float MoveSpeed = 5f;
    public int Damage = 1;
    public float rotateDamping = 1;
    public float targetTolerance = 1;
}
