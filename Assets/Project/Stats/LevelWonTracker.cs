using UnityEngine;

[CreateAssetMenu(menuName = "SO/Stats/Level Won")]
public class LevelWonTracker : LevelTracker
{
    protected override void InitTracker()
    {
        GameStateManager.onGameWin += OnLevelComplete;
    }

    public override void Print()
    {
        Debug.Log($"{levelToTrack.levelTitle} completed: {total}");
    }

    private void OnLevelComplete()
    {
        total++;
        
    }
}