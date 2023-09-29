using System;
using Project.Towers.Scripts;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(HealthController))]
public class Tower : MonoBehaviour, IEnemyTargetable
{
    public HealthController healthController;
    
    protected bool isPlayerControlled = false;
    [SerializeField] private Transform playerControlPosition;
    [SerializeField] private Transform playerReleasePosition;
    [SerializeField] private GameObject deathParticles;

    public Tower_SO dto;

    public static event Action<Tower> OnTowerSpawn;
    public static event Action<Tower> OnTowerDestroy; 

    #region Unity Interface

    // Start is called before the first frame update
    protected virtual void Awake()
    {
        if(healthController == null)
            healthController = GetComponent<HealthController>();
        healthController.onDeath.AddListener(Die);
        if (deathParticles)
            deathParticles.SetActive(false);
    }

    [HideInInspector] public bool removeFromDict = true;
    private void OnDestroy()
    {
        Vector3 pos = transform.position;
        TowerSpawnManager._towersByPos.Remove(pos);
        if(Minimap.instance)
            Minimap.instance.DestroyTowerAt(pos);
        TowerSpawnManager._towersByPos.Remove(pos);
        
        if(Minimap.instance)
            Minimap.instance.DestroyTowerAt(pos);
        if (removeFromDict)
        {
            TowerSpawnManager._towersByPos.Remove(pos);
            
            if(Minimap.instance)
                Minimap.instance.DestroyTowerAt(pos);
        }
    }

    #endregion


    #region Tower Takeover

    public virtual void Selected()
    {
        BubbleMenuController.Open(this);
    }    
    public virtual void Deselected()
    {
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

    public virtual void Die()
    {
        if (isPlayerControlled)
        {
            PlayerStateController.ReleaseControlOfTower();
            PlayerStateController.instance.TeleportPlayerToPenthouse();
            PlayerReleaseControl();
            InventoryManager.instance.ReleaseAllItems();
        }
        if (deathParticles == null)
        {
            GameObject g = gameObject;
            Debug.LogError($"{g.name} had no death particles", g);
            Destroy(gameObject,.01f);
            return;
        }
        deathParticles.SetActive(true);
        deathParticles.transform.parent = null;
        Destroy(deathParticles, 5f);
        Destroy(gameObject,.01f);
    }

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