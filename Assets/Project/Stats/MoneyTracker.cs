using UnityEngine;

[CreateAssetMenu(menuName = "SO/Stats/Money Earned")]
public class MoneyTracker : StatTracker
{
    public override void Initialize()
    {
        throw new System.NotImplementedException();
    }
    
    public override void Print()
    {
        Debug.Log($"Total money: {total}");
    }
}