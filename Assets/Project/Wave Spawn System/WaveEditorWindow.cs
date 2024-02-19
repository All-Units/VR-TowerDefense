#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Callbacks;
using UnityEngine;
using Object = UnityEngine.Object;

public class WaveEditorWindow : EditorWindow
{
    [MenuItem("Castle Tools/Wave Editor %#w")]
    public static void OpenWindow()
    {
        wave_i = -1;
        var window = GetWindow<WaveEditorWindow>();

        window.Show();

    }
    static LevelSpawn_SO currentLevel;
    Vector2 pos;
    [SerializeField]
    public List<SpawnPointData> spawnPoints;


    bool _IsDisplayingPrefabs = false;
    
    private void OnGUI()
    {
        
        if (GUILayout.Button("Open Level", _centerButton))
        {
            string path = EditorUtility.OpenFilePanel("Open level", "Assets/Project/Wave Spawn System/Levels", "asset");
            if (path.Length != 0)
            {
                path = path.Replace(Application.dataPath, "Assets/");
                var content = AssetDatabase.LoadAssetAtPath<LevelSpawn_SO>(path);
                currentLevel = content;
            }
        }
        if (currentLevel == null)
            return;
        if (currentLevel.enemyPrefabs == null || currentLevel.enemyPrefabs.Count == 0)
        {
            currentLevel.enemyPrefabs = new List<EnemyPrefabByType>(4) {};
        }
        string display = _IsDisplayingPrefabs ? "Hide prefabs" : "Display prefabs";
        if (GUILayout.Button(display))
            _IsDisplayingPrefabs = !_IsDisplayingPrefabs;
        
        foreach (int j in Enum.GetValues(typeof(EnemyType)))
        {
            if (_IsDisplayingPrefabs == false) break;
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label($"{(EnemyType)j}:");
            while (j >= currentLevel.enemyPrefabs.Count)
                currentLevel.enemyPrefabs.Add(new EnemyPrefabByType());
            Object greg = EditorGUILayout.ObjectField((Object)currentLevel.enemyPrefabs[j].prefab, typeof(GameObject), true);
            
            EditorGUILayout.EndHorizontal();
            if (greg != currentLevel.enemyPrefabs[j].prefab || currentLevel.enemyPrefabs[j].type != (EnemyType)j)
            {
                var e = currentLevel.enemyPrefabs[j];
                e.type = (EnemyType)j;
                e.prefab = (GameObject)greg;
                currentLevel.enemyPrefabs[j] = e;
                _dirty();
                break;
            }
        }
        

        GUILayout.Label($"Current Level: {currentLevel.name}");
        pos = GUILayout.BeginScrollView(pos);

        int i = 0;
        foreach (var wave in currentLevel.waveStructs)
        {
            GUI.contentColor = Color.green;
            GUILayout.Label($"\n-------------------------------------------------------------------------------------------\n" +
                $"Wave {i + 1}\n", _centerLabel);
            GUI.contentColor = Color.white;
            GUILayout.BeginHorizontal();

            if (GUILayout.Button($"Wave {i + 1}", _centerButton))
            {
                wave_i = i;
            }
            var delay = currentLevel.GetDelay(i);
            var newDelay = EditorGUILayout.IntField("Pre wave delay: ", delay);
            int bounty = currentLevel.GetBounty(i);
            var newBounty = EditorGUILayout.IntField("Wave Complete Bounty", bounty);
            if (GUILayout.Button("X", _xButton))
            {
                currentLevel.waveStructs.RemoveAt(i);
                _dirty();
                GUILayout.EndHorizontal();
                GUILayout.EndScrollView();
                break;
            }
            GUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();


            Vector2Int groupSizes = currentLevel.GetGroupSizes(i);
            var newSizes = groupSizes;//EditorGUILayout.Vector2IntField("Group sizes:", groupSizes);
            newSizes.x = EditorGUILayout.IntField("Min group", newSizes.x);
            newSizes.y = EditorGUILayout.IntField("Max group", newSizes.y);

            float spawnRate = currentLevel.GetSpawnRate(i);
            float newRate = EditorGUILayout.FloatField("Spawn rate (secs between gregs)", spawnRate);
            //Min group size is 1
            if (newSizes.x < 1 || newSizes.y < 1)
                newSizes = Vector2Int.one;
            //If the group sizes changed, set x to min and y to max
            if (groupSizes != newSizes)
            {
                //If X changed, and it's larger than y, bring y up to match
                if (newSizes.x != groupSizes.x && newSizes.x > newSizes.y)
                    newSizes.y = newSizes.x;
                //If Y changed, and is less than x, drop x to match
                if (newSizes.y != groupSizes.y && newSizes.y < newSizes.x)
                    newSizes.x = newSizes.y;
            }

            EditorGUILayout.EndHorizontal();
            bool save = false;
            if (GUILayout.Button("Save spawn points", _centerButton))
                save = true;
            spawnPoints = currentLevel.waveStructs[i].spawnPoints;
            SerializedObject so = new SerializedObject(this);
            SerializedProperty spawnProperty = so.FindProperty("spawnPoints");
            EditorGUILayout.PropertyField(spawnProperty, new GUIContent("Spawn points:"), true);
            so.ApplyModifiedProperties();
            if (bounty != newBounty || save || spawnRate != newRate
                || delay != newDelay || groupSizes != newSizes)
            {
                currentLevel.EditBounty(i, newBounty);
                currentLevel.EditDelay(i, newDelay);
                currentLevel.EditGroupSizes(i, newSizes);
                currentLevel.EditSpawnRate(i, newRate);
                var wv = currentLevel.waveStructs[i];
                wv.spawnPoints = spawnPoints;
                currentLevel.waveStructs[i] = wv;
                so.ApplyModifiedProperties();
                _dirty();
                break;
            }

            if (wave_i == i)
                DisplayWave();
            i++;
        }

        if (GUILayout.Button("New Wave", _centerButton))
        {
            currentLevel.waveStructs.Add(new WaveStruct());
            _dirty();
        }
        GUILayout.EndScrollView();
        
    }
    static int wave_i = -1;
    void DisplayWave()
    {
        GUILayout.Label("\n");
        GUILayout.Label($"Current wave: {wave_i + 1}");
        GUILayout.Label("\n");
        var wave = currentLevel.waveStructs[wave_i];
        
        DisplayEnemies(wave.enemies);
        
        GUILayout.Label("\n\n");
        GUI.contentColor = Color.red;
        if (wave.subWaves == null || wave.subWaves.Count == 0)
        {
            GUILayout.Label("No subwaves", _centerLabel);
            GUI.contentColor = Color.white;
        }
        else
        {
            GUILayout.Label($"{wave.subWaves.Count} subwaves", _centerLabel); 
            GUI.contentColor = Color.white;
            DisplaySubwaves(wave);
        }
        
        if (GUILayout.Button("Add Subwave", _centerButton))
        {
            wave.subWaves.Add(new SubWave());
            _dirty();
        }
        GUILayout.Label("\n\n");
    }
    List<EnemyQuant> DisplayEnemies(List<EnemyQuant> enemies)
    {
        int i = 0;
        bool changed = false;
        if (enemies == null) enemies = new List<EnemyQuant> { };
        if (enemies.Count == 0)
        {
            var quant = new EnemyQuant();
            quant.amountToSpawn = Vector2Int.one;
            enemies.Add(quant);
        }
        foreach (var enemy in enemies)
        {
            EditorGUILayout.BeginHorizontal();
            EnemyType type = (EnemyType)EditorGUILayout.EnumPopup(enemy.enemyType);
            Vector2Int startAmount = enemy.amountToSpawn;
            var amount = startAmount;
            amount.x = EditorGUILayout.IntField("Min amount:", amount.x);
            amount.y = EditorGUILayout.IntField("Max amount:", amount.y);
            bool removed = false;
            if (GUILayout.Button("X", _xButton))
            {
                removed = true;
            }
            EditorGUILayout.EndHorizontal();
            
            if (type != enemy.enemyType)
            {
                var e = enemies[i];
                e.enemyType = type;
                enemies[i] = e;
                changed = true;
            }
            if (amount != startAmount)
            {
                Vector2Int updatedAmount = amount;
                //If x is larger, bring y up to match
                if (updatedAmount.x != startAmount.x && updatedAmount.x > updatedAmount.y)
                    updatedAmount.y = updatedAmount.x;
                //If y is smaller, drop x to match
                if (updatedAmount.y != startAmount.y && updatedAmount.y < updatedAmount.x)
                    updatedAmount.x = updatedAmount.y;
                var e = enemies[i];
                e.amountToSpawn = updatedAmount;
                enemies[i] = e;
                changed = true;

            }
            if (removed)
            {
                enemies.RemoveAt(i);
                changed = true;
            }
            if (changed)
            {
                _dirty();
                break;
            }


            i++;
        }
        GUILayout.Label("\n");
        if (GUILayout.Button("Add Enemy", _centerButton))
        {
            enemies.Add(new EnemyQuant());
            _dirty();
        }
        return enemies;
    }

    void DisplaySubwaves(WaveStruct wave)
    {
        bool changed = false;
        int i = 0;
        foreach (var subWave in wave.subWaves)
        {
            GUI.contentColor = Color.red;
            GUILayout.Label($"\n---------------------------\n" +
                $"SubWave {i + 1}\n\n", _centerLabel);
            GUI.contentColor = Color.white;
            if (GUILayout.Button("Save spawn points",_centerButton))
                changed = true;
            spawnPoints = subWave.spawnPoints;
            var target = this;

            SerializedObject so = new SerializedObject(target);
            
            SerializedProperty spawnProperty = so.FindProperty("spawnPoints");
            EditorGUILayout.PropertyField(spawnProperty, new GUIContent("Spawn points:"), true);
            so.ApplyModifiedProperties();
            GUILayout.BeginHorizontal();    
            DelayType delay = subWave.delayType;
            delay = (DelayType)EditorGUILayout.EnumPopup("Delay type: ", delay);
            int count = subWave.DelayCount;
            count = EditorGUILayout.IntField("Delay amount: ", count);
            bool removed = false;
            if (GUILayout.Button("X", _xButton))
            {
                removed = true;
            }

            GUILayout.EndHorizontal();


            //Min / max group, and spawn rate
            GUILayout.BeginHorizontal();
            var sizes = subWave.groupSizes;
            if (sizes.x < 1 || sizes.y < 1)
                sizes = Vector2Int.one;
            int min = EditorGUILayout.IntField("Min group", sizes.x);
            int max = EditorGUILayout.IntField("Max group", sizes.y);
            sizes = new Vector2Int(min, max);
            var rate = EditorGUILayout.FloatField("Spawn Rate (sec)", subWave.spawnRate);
            GUILayout.EndHorizontal();

            if (delay != subWave.delayType)
            {
                var s = wave.subWaves[i];
                s.delayType = delay;
                wave.subWaves[i] = s;
                changed = true;
            }
            if (count != subWave.DelayCount)
            {
                var s = wave.subWaves[i];
                s.DelayCount = count;
                wave.subWaves[i] = s;
                changed = true;
            }
            if (rate != subWave.spawnRate || sizes != subWave.groupSizes)
            {
                var s = wave.subWaves[i];
                s.spawnRate = rate;
                s.groupSizes = sizes;
                wave.subWaves[i] = s;
                changed = true;
            }
            if (removed)
            {
                wave.subWaves.RemoveAt(i);
                changed = true;
            }

            GUILayout.Label("\n");
            var e = DisplayEnemies(subWave.enemies);
            
            if (e != subWave.enemies)
            {
                changed = true;
                var subwave = wave.subWaves[i];
                subwave.enemies = e;
                wave.subWaves[i] = subwave;
            }
            if (changed)
            {
                _dirty();
                break;
            }
            i++;
            
        }
    }

    /// <summary>
    /// Centers a given label
    /// </summary>
    GUIStyle _centerLabel { get{
            var centeredStyle = GUI.skin.GetStyle("Label");
            centeredStyle.alignment = TextAnchor.UpperCenter;
            return centeredStyle;
        }
        } 
    /// <summary>
    /// Centers a given button
    /// </summary>
    GUIStyle _centerButton { get{
            var centeredStyle = GUI.skin.GetStyle("Button");
            centeredStyle.alignment = TextAnchor.UpperCenter;
            centeredStyle.fixedWidth = 300;
            return centeredStyle;
        }
        }
    GUIStyle _xButton { get{
            var centeredStyle = GUI.skin.GetStyle("Button");
            centeredStyle.fixedWidth = 25;
            centeredStyle.alignment = TextAnchor.MiddleLeft;
            
            return centeredStyle;
        }
        }


    [OnOpenAsset]
    public static bool OnDoubleClickSO(int instanceID, int line)
    {
        Object target = EditorUtility.InstanceIDToObject(instanceID);
        if (target is LevelSpawn_SO level)
        {
            Debug.Log("Double clicked level spawn SO");
            currentLevel = level;
            OpenWindow();
            return true;
        }
        return false;
    }
    void _dirty()
    {
        EditorUtility.SetDirty(currentLevel);
    }
}
#endif