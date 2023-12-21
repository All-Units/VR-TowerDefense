using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class TowerSOEditorWindow : EditorWindow
{
    [MenuItem("Castle Tools/Tower SO Editor", false, 99)]
    public static void OpenWindow()
    {
        var window = GetWindow<TowerSOEditorWindow>("Tower SO Editor");
    }

    private Vector2 scrollPosition;
    private List<Tower_SO> towerSOs;

    private void OnEnable()
    {
        towerSOs = new List<Tower_SO>();
        string[] guids = AssetDatabase.FindAssets("t:Tower_SO");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Tower_SO towerSO = AssetDatabase.LoadAssetAtPath<Tower_SO>(path);
            if (towerSO != null)
            {
                towerSOs.Add(towerSO);
            }
        }
    }

    private void OnGUI()
    {
        GUILayout.Label("Tower Scriptable Objects", EditorStyles.boldLabel);

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        EditorGUILayout.BeginVertical();

        foreach (Tower_SO towerSO in towerSOs)
        {
            EditorGUILayout.BeginHorizontal("box");

            // Display select fields (e.g., cost and maxHealth) in columns
            EditorGUILayout.LabelField(towerSO.name, GUILayout.Width(100));
            EditorGUILayout.LabelField("|", GUILayout.Width(10));
            towerSO.cost = EditorGUILayout.IntField("Cost: $", towerSO.cost, GUILayout.Width(200));
            towerSO.maxHeath = EditorGUILayout.IntField("Max Health: ", towerSO.maxHeath, GUILayout.Width(200));
            if (towerSO is ProjectileTower_SO projectileTowerSo)
            {
                projectileTowerSo.radius = EditorGUILayout.FloatField("Radius: ", projectileTowerSo.radius, GUILayout.Width(200));
                projectileTowerSo.shotCooldown = EditorGUILayout.FloatField("Cooldown: ", projectileTowerSo.shotCooldown, GUILayout.Width(200));
            }
            else
            {
                EditorGUILayout.LabelField("", GUILayout.Width(403));
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndScrollView();
    }
}