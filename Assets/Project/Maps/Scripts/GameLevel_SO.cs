using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Levels/Game Level")]
public class GameLevel_SO : ScriptableObject
{
#if UNITY_EDITOR
    public SceneAsset scene;
#endif 
    public string levelTitle;
    [Multiline]
    public string levelDescription;
}