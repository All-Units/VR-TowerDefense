using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(StatDisplayModel))]
public class StatDisplayModelEditor : Editor
{
    private StatDisplayModel s => ((target != null) ? (StatDisplayModel)target : null);
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (s != null)
        {
            foreach (var stat in s.statTrackers)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label($"{stat.statName} : {Mathf.Max(stat.getSerializeValue, stat.total)}");
                if (stat is TowerDestroyedTracker tower)
                {
                    GUILayout.Label($"{tower.LostAsPlayerSuffix} : {tower.DestroyedAsPlayerCount}");
                }
                EditorGUILayout.EndHorizontal();
            }
        }
    }
}


[CreateAssetMenu(menuName = "SO/Stats/Display")]
public class StatDisplayModel : ScriptableObject
{
    public string displayName;
    public List<StatTracker> statTrackers = new();
}