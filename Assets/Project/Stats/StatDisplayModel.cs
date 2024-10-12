using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

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
                if (stat == null) continue;
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
#endif

[CreateAssetMenu(menuName = "SO/Stats/Display")]
public class StatDisplayModel : ScriptableObject
{
    public string displayName;
    public List<StatTracker> statTrackers = new();
}