using UnityEngine;

public class SpawnPointData : ScriptableObject
{
    public Vector3 pos;
    public Transform enemyParent;
    public SpawnPoint SpawnPoint;
    /// <summary>
    /// Returns the position, offset by a random amount within bounds
    /// </summary>
    /// <param name="offset">The max amount of be offset by</param>
    /// <returns></returns>
    public Vector3 PositionOffset(float offset)
    {
        Vector3 position = pos;
        position += new Vector3(Random.Range(-offset, offset), 0.1f, Random.Range(-offset, offset));
        return position;
    }
    
}

