using System;
using System.Collections;
using Project.Towers.Scripts;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

[RequireComponent(typeof(HealthController))]
public class Tower : MonoBehaviour, IEnemyTargetable, IPausable
{
    public HealthController healthController;
    
    [Header("Tower VFX")] 
    [SerializeField] private GameObject deathParticles;
    [SerializeField] protected GameObject selectedVfx;

    public bool IsInitialized => isInitialized;
    [SerializeField] protected bool isInitialized = false;
    protected float buildTime = 2.5f;

    public Tower_SO dto;

    public static event Action<Tower> OnTowerSpawn;
    public static event Action<Tower> OnTowerDestroy;
    public static event Action<Tower> OnTowerSelected; 
    public event Action OnSelected; 
    public UnityEvent onStartFocus;
    public static event Action<Tower> OnTowerDeselected; 
    public event Action OnDeselected; 
    public UnityEvent onEndFocus;


    #region Unity Interface

    // Start is called before the first frame update
    protected virtual void Awake()
    {
        if(healthController == null)
            healthController = GetComponent<HealthController>();
        healthController.onDeath.AddListener(Die);
        if (deathParticles)
            deathParticles.SetActive(false);
        selectedVfx.SetActive(false);
    }

    [HideInInspector] public bool removeFromDict = true;

    private IPausableComponents _ipComponents = null;
    public IPausableComponents IPComponents
    {
        get
        {
            if (_ipComponents == null) _ipComponents = this.GetPausableComponents();
            return _ipComponents;
        }
    }

    private void OnDestroy()
    {
        OnDestroyPausable();
        
    }

    #endregion

    #region Tower Lifecycle

    public void SpawnTower()
    {
        OnInitPausable();
        StartCoroutine(PlayBuildingAnimation());
        
        OnTowerSpawn?.Invoke(this);
    }

    private IEnumerator PlayBuildingAnimation()
    {
        transform.localScale = Vector3.one;

        var director = GetComponentInChildren<PlayableDirector>();
        if (director != null )
            yield return new WaitForSeconds((float)director.duration);
        
        isInitialized = true;
        healthController.SetMaxHealth(dto.maxHeath);
    }

    #endregion

    #region Tower Takeover

    public virtual void Selected()
    {
        OnTowerSelected?.Invoke(this);
        OnSelected?.Invoke();
        selectedVfx.SetActive(true);
    }    
    public virtual void Deselected()
    {
        OnTowerDeselected?.Invoke(this);
        OnDeselected?.Invoke();
        selectedVfx.SetActive(false);
    }

    public void StartFocus()
    {
        onStartFocus?.Invoke();
    }

    public void EndFocus()
    {
        onEndFocus?.Invoke();
    }

    #endregion

    public virtual void Die()
    {
        
        TowerSpawnManager.PlayDeathSounds(transform.position);

        if (deathParticles == null)
        {
            GameObject g = gameObject;
            //Debug.LogError($"{g.name} had no death particles", g);
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

    public void OnInitPausable()
    {
        this.InitPausable();
    }

    public void OnDestroyPausable()
    {
        this.DestroyPausable();
    }

    public void OnPause()
    {
        this.BaseOnPause();
    }

    public void OnResume()
    {
        this.BaseOnResume();
    }

    #endregion
}