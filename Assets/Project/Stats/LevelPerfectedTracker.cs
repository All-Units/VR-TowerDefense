using UnityEngine;

[CreateAssetMenu(menuName = "SO/Stats/Level Perfected")]
public class LevelPerfectedTracker : LevelTracker
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
        if(Gate.IsFullHealth)
            total++;
    }

    public override void ClearTracker()
    {
        GameStateManager.onGameWin -= OnLevelComplete;
    }
}