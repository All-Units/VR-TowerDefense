using UnityEngine;

public abstract class StatTracker : ScriptableObject
{ 
    public string key;
    public string displayName;
    public string statName;

    public int total = 0;
    private bool _isInitialized = false;

    public void Initialize(bool force = false)
    {
        if(_isInitialized && force == false)
            return;
        InitTracker();
        _isInitialized = true;
    }

    protected abstract void InitTracker();

    public void ResetTotal()
    {
        total = 0;
    }

    public int getSerializeValue => PlayerPrefs.GetInt(key, 0);
    
    public void Serialize()
    {
        PlayerPrefs.SetInt(key, total);
    }
    
    public void Deserialize()
    {
        total = PlayerPrefs.GetInt(key);
    }

    public abstract void Print();
}

public class TowerPlayerWeaponKillsTracker : StatTracker
{
    [SerializeField] TowerTakeoverObject trackedTowerTakeoverObject;
    protected override void InitTracker()
    {
        TowerTakeoverObject.OnKillWithItem += OnKill;
    }

    public override void Print()
    {
        Debug.Log($"{statName}: {total}");
    }

    private void OnKill(TowerTakeoverObject towerTakeoverObject)
    {
        if(trackedTowerTakeoverObject == towerTakeoverObject)
            total++;
    }
}