using System;
using UnityEngine;

public class CastleController : MonoBehaviour
{
    private static CastleController instance;

    [SerializeField] private Transform endGamePoint;

    private void Awake()
    {
        instance = this;
    }

    public static Transform GetEndgamePoint()
    {
        return instance ? instance.endGamePoint : null;
    }
}
