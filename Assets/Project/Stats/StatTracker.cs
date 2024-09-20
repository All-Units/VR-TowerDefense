using UnityEngine;

public abstract class StatTracker : ScriptableObject
{
    public string key;
    public string displayName;
    public string statName;

    public int total = 0;
    [SerializeField] private bool _isInitialized = false;

    public void Initialize(bool _force = false)
    {
        if (_force)
            _isInitialized = false;
        if (_isInitialized) return;

        InitTracker();
        _isInitialized = true;
    }

    protected abstract void InitTracker();
    public abstract void ClearTracker();

    public void ResetTotal()
    {
        total = 0;
    }

    public int getSerializeValue => PlayerPrefs.GetInt(key, 0);
    public void SerializeIfChanged()
    {
        if (total == getSerializeValue) return;
        Serialize();
    }
    
    public void Serialize()
    {
        PlayerPrefs.SetInt(key, total);
    }
    
    public void Deserialize()
    {
        total = getSerializeValue;
    }

    public abstract void Print();
}