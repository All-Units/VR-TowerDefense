using UnityEngine;

[CreateAssetMenu(menuName = "SO/Stats/Level Played")]
public class LevelPlayedTracker : LevelTracker
{

    protected override void InitTracker()
    {
        OnLevelStarted();
    }

    public override void Print()
    {
        Debug.Log($"{levelToTrack.levelTitle} completed: {total}");
    }

    private void OnLevelStarted()
    {
        total++;
    }
}