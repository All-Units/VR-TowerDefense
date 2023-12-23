#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ProjectileTower_SO))]
public class ProjectileTower_SO_Inspector : Editor
{
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Set From Stats"))
            SetDataFromStats();
            
        base.OnInspectorGUI();
        
        if (Event.current.commandName == "ObjectSelectorClosed")
        {   
            if (EditorGUIUtility.GetObjectPickerObject() is TowerStats towerStats)
            {
                var pt = (ProjectileTower_SO)target;
                // Copy the values from the selected TowerStats object
                pt.radius = towerStats.radius;
                pt.shotCooldown = towerStats.shotCooldown;
                pt.projectile = towerStats.projectile;
                // Copy other relevant fields
                EditorUtility.SetDirty(pt);
            }
        }
    }
    
    private void SetDataFromStats()
    {
        EditorGUIUtility.ShowObjectPicker<TowerStats>(null, false, "t:TowerStats", 0);
    }
}

#endif