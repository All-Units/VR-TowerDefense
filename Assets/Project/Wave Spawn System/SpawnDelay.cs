using System;
using System.Threading.Tasks;
using UnityEngine;

public static class DelayFactory
{
    public static SpawnDelay Construct(DelayManifest manifest)
    {
        return manifest.delayType switch
        {
            SpawnDelay.DelayType.Timed => new TimedSpawnDelay(manifest.val),
            SpawnDelay.DelayType.EnemyCount => new EnemyCountDelay(manifest.val),
            _ => new TimedSpawnDelay(0)
        };
    }
}

[Serializable]
public struct DelayManifest
{
    public SpawnDelay.DelayType delayType;
    public float val;
}

[Serializable]
public abstract class SpawnDelay
{
    public abstract void StartDelay();
    public abstract bool IsComplete();

    public enum DelayType
    {
        Timed,
        EnemyCount
    }
}

[Serializable]
public class TimedSpawnDelay : SpawnDelay
{
    [SerializeField] private float delayTime;
    private bool _isComplete = false;

    public TimedSpawnDelay(float delayTime)
    {
        this.delayTime = delayTime;
    }

    public override void StartDelay()
    {
        Debug.Log($"Delaying for {delayTime}s!");
        DelayForSeconds();
    }

    public override bool IsComplete() => _isComplete;

    private async void DelayForSeconds()
    {
        await Task.Delay((int)(delayTime * 1000));
        _isComplete = true;
        Debug.Log("Delay Complete!");
    }
}

[Serializable]
public class EnemyCountDelay : SpawnDelay
{
    [SerializeField] private int enemiesRemaining;

    public EnemyCountDelay(float val)
    {
        enemiesRemaining = (int)val;
    }

    public override void StartDelay()
    {
        
    }

    public override bool IsComplete()
    {
        // Todo query the currently spawned enemies
        return true;
    }
}