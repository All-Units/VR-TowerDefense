using UnityEngine;

[CreateAssetMenu(menuName = "SO/Stats/Money Earned")]
public class MoneyTracker : StatTracker
{
    protected override void InitTracker()
    {
        
    }
    
    public override void Print()
    {
        Debug.Log($"Total money: {total}");
    }
}