#if UNITY_EDITOR


using System;
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
            bool isDirty = false;
            EditorGUILayout.BeginHorizontal("box");

            // Display select fields (e.g., cost and maxHealth) in columns
            EditorGUILayout.LabelField(towerSO.name, GUILayout.Width(100));
            EditorGUILayout.LabelField("|", GUILayout.Width(10));
            EditorGUIUtility.labelWidth = 75;
            var cost = EditorGUILayout.IntField("Cost: $", towerSO.cost, GUILayout.Width(150));
            if (cost != towerSO.cost)
            {
                towerSO.cost = cost;
                isDirty = true;
            }
            
            var health = EditorGUILayout.IntField("Max Health: ", towerSO.maxHeath, GUILayout.Width(150));
            if (health != towerSO.maxHeath)
            {
                towerSO.maxHeath = health;
                isDirty = true;
            }
            
            if (towerSO is ProjectileTower_SO projectileTowerSo)
            {
                var radius = EditorGUILayout.FloatField("Radius: ", projectileTowerSo.radius, GUILayout.Width(200));
                if (Math.Abs(radius - projectileTowerSo.radius) > 0.01f)
                {
                    projectileTowerSo.radius = radius;
                    isDirty = true;
                }
                
                var shotCooldown = EditorGUILayout.FloatField("Cooldown: ", projectileTowerSo.shotCooldown, GUILayout.Width(200));
                if (Math.Abs(shotCooldown - projectileTowerSo.shotCooldown) > 0.01f)
                {
                    projectileTowerSo.shotCooldown = shotCooldown;
                    isDirty = true;
                }
                
                var damage = EditorGUILayout.IntField("Damage", projectileTowerSo.projectile.damage);
                if (damage != projectileTowerSo.projectile.damage)
                {
                    projectileTowerSo.projectile.damage = damage;
                    EditorUtility.SetDirty(projectileTowerSo.projectile);
                }
                var damageVar = EditorGUILayout.IntField("Damage var", projectileTowerSo.projectile.DamageVariability);
                if (damageVar != projectileTowerSo.projectile.DamageVariability)
                {
                    projectileTowerSo.projectile.DamageVariability = damageVar;
                    EditorUtility.SetDirty(projectileTowerSo.projectile);
                }   
                
                var speed = EditorGUILayout.FloatField("Speed", projectileTowerSo.projectile.speed);
                if (Math.Abs(speed - projectileTowerSo.projectile.speed) > 0.01f)
                {
                    projectileTowerSo.projectile.speed = speed;
                    EditorUtility.SetDirty(projectileTowerSo.projectile);
                }
                
                if (projectileTowerSo.PlayerProjectile != null)
                {
                    var arrow = projectileTowerSo.PlayerProjectile.GetComponent<Arrow>();
                    if (arrow != null)
                    {
                        int playerDmg = EditorGUILayout.IntField("Player dmg: ", arrow.damage);
                        if (playerDmg != arrow.damage)
                        {
                            arrow.damage = playerDmg;
                            EditorUtility.SetDirty(arrow);
                        }
                    }
                    var proj = projectileTowerSo.PlayerProjectile.GetComponent<Projectile>();
                    if (proj != null)
                    {
                        int playerDmg = EditorGUILayout.IntField("Player dmg: ", proj.damage);
                        if (playerDmg != proj.damage)
                        {
                            proj.damage = playerDmg;
                            EditorUtility.SetDirty(proj);
                        }
                        _SetAoERange(proj);
                    }
                    
                }
            }
            else
            {
                EditorGUILayout.LabelField("", GUILayout.Width(403));
            }

            EditorGUILayout.EndHorizontal();

            if (isDirty)
                EditorUtility.SetDirty(towerSO);
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndScrollView();
    }
    void _SetAoERange(Projectile proj)
    {
        if (proj is AOEProjectile aoe)
        {

        }
        else
            return;
        float radius = EditorGUILayout.FloatField("AoE radius:", aoe.splashRadius);
        if (radius != aoe.splashRadius)
        {
            aoe.splashRadius = radius;
            EditorUtility.SetDirty(aoe);
        }
    }
}



#endif