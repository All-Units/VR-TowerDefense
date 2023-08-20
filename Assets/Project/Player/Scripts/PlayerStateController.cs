using UnityEngine;

public class PlayerStateController : MonoBehaviour
{
    public static PlayerStateController instance;

    private void Awake()
    {
        instance = this;
    }

    public static void TakeControlOfTower(Tower tower)
    {
        
    }
}
