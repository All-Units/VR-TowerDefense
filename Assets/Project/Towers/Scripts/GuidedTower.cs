
public class GuidedTower : ProjectileTower
{
    public Enemy target;
    
    private void Start()
    {
        targetingSystem.OnEnter += OnEnter;
        targetingSystem.OnExit += OnExit;
    }

    private void OnEnter(Enemy obj)
    {
        
    }
    
    private void OnExit(Enemy obj)
    {
        
    }
}