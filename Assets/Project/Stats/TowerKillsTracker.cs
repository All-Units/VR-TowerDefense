using UnityEngine;

[CreateAssetMenu(menuName = "SO/Stats/Tower Kills")]
public class TowerKillsTracker : TowerTracker
{
    protected override void InitTracker()
    {
        ProjectileTower.onKill += ProjectileTowerOnKill;
    }

    private void ProjectileTowerOnKill(ProjectileTower arg1, Enemy arg2)
    {
        if(_towerToTrack == arg1.dto)
            total++;
    }

    public override void Print()
    {
        Debug.Log($"{_towerToTrack.name} placed: {total}");
    }
}