using System;
using System.Collections;
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
    [SerializeField] private ParticleSystem constructionParticles;

    [SerializeField] protected bool isInitialized = false;
    protected float buildTime = 2.5f;

    public Tower_SO dto;

    public static event Action<Tower> OnTowerSpawn;
    public static event Action<Tower> OnTowerDestroy;
    public static event Action<Tower> OnTowerSelected; 
    public event Action OnSelected; 
    public static event Action<Tower> OnTowerDeselected; 
    public event Action OnDeselected; 

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

    private void OnEnable()
    {
        //Minimap.SpawnTower(transform.position, dto);
    }

    [HideInInspector] public bool removeFromDict = true;
    private void OnDestroy()
    {
        Vector3 pos = transform.position;
        TowerSpawnManager._towersByPos.Remove(pos);
        if (removeFromDict)
        {
            Minimap.DestroyTower(pos);
        }
    }

    #endregion

    #region Tower Lifecycle

    public void SpawnTower()
    {
        StartCoroutine(PlayBuildingAnimation());
        OnTowerSpawn?.Invoke(this);
    }

    private IEnumerator PlayBuildingAnimation()
    {
        var time = buildTime;
        if(constructionParticles)
            constructionParticles.Play();

        while (time > 0)
        {
            transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, time / buildTime);
            time -= Time.deltaTime;
            yield return null;
        }

        if(constructionParticles)
            constructionParticles.Stop();

        transform.localScale = Vector3.one;
        isInitialized = true; 
    }

    #endregion

    #region Tower Takeover

    public virtual void Selected()
    {
        OnTowerSelected?.Invoke(this);
        OnSelected?.Invoke();
    }    
    public virtual void Deselected()
    {
        OnTowerDeselected?.Invoke(this);
        OnDeselected?.Invoke();
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
        TowerSpawnManager.PlayDeathSounds(transform.position);
        if (isPlayerControlled)
        {
            PlayerStateController.DiedInTower();
            
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