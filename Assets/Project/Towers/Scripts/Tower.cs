using Project.Towers.Scripts;
using UnityEngine;

[RequireComponent(typeof(HealthController))]
public class Tower : MonoBehaviour, IEnemyTargetable
{
    public HealthController healthController;
    
    protected bool isPlayerControlled = false;
    [SerializeField] private Transform playerControlPosition;
    [SerializeField] private Transform playerReleasePosition;


    #region Unity Interface

    // Start is called before the first frame update
    protected virtual void Awake()
    {
        if(healthController == null)
            healthController = GetComponent<HealthController>();
    }

    private void OnDestroy()
    {
        TowerSpawnManager._towersByPos.Remove(transform.position);
    }

    #endregion


    #region Tower Takeover

    public virtual void Selected()
    {
        Debug.LogError($"No Override implemented for Selected() on {gameObject}", gameObject);
    }    
    public virtual void Deselected()
    {
        Debug.LogError($"No Override implemented for Deselected() on {gameObject}", gameObject);
    }
    
    public virtual void PlayerTakeControl()
    {
        isPlayerControlled = true;
    }

    public virtual void PlayerReleaseControl()
    {
        isPlayerControlled = false;
    }

    public Transform GetPlayerControlPoint()
    {
        return playerControlPosition;
    }

    public Transform GetPlayerEjectPoint()
    {
        return playerReleasePosition;
    }

    #endregion
    

    #region IEnemyTargetable Interface

        public HealthController GetHealthController()
        {
            return healthController;
        }
    
        public Vector3 GetPosition()
        {
            return transform.position;
        }

    #endregion

}