using UnityEngine;

public abstract class StatTracker : ScriptableObject
{ 
    public string key;
    public string displayName;
    public string statName;

    public int total = 0;

    public abstract void Initialize();

    public void ResetTotal()
    {
        total = 0;
    }

    public int getSerializeValue => PlayerPrefs.GetInt(key, 0);

    public abstract void Print();
}