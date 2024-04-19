using UnityEngine;
using UnityEngine.Serialization;

public abstract class LevelTracker : StatTracker
{ 
    [SerializeField] protected GameLevel_SO levelToTrack;
    public GameLevel_SO LevelToTrack => levelToTrack;
}