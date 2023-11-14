using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Levels/Game Level")]
public class GameLevel_SO : ScriptableObject
{
    public SceneAsset scene;
    public string levelTitle;
    [Multiline]
    public string levelDescription;
}