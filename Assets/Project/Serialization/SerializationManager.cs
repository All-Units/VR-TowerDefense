using Project.Towers.Scripts;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class SerializationManager : MonoBehaviour
{
    public LevelSelectData levelSelectData;
    public List<Tower_SO> TowerTypes;
    private static SerializationManager instance;

    private static LevelSelectData _levelToLoad = null;

    private string _path => $"{Application.persistentDataPath}/{levelSelectData.sceneName}.dat";

    private void Start()
    {
        instance = this;

        if (_levelToLoad != null)
        {
            print($"Was told to load level, loading {levelSelectData.name} save file");
            LoadGame();
            _levelToLoad = null;
        }

        EnemyManager.OnRoundStarted.AddListener(SaveGame);
        GameStateManager.onStartGameWin += _RemoveSave;
        GameStateManager.onStartGameLose += _RemoveSave;
    }

    private void OnDestroy()
    {
        EnemyManager.OnRoundStarted.RemoveListener(SaveGame);
        GameStateManager.onStartGameWin -= _RemoveSave;
        GameStateManager.onStartGameLose -= _RemoveSave;
    }

    private void _RemoveSave()
    {
        if (_SaveFileExists() == false) return;
        File.Delete(_path);
    }
    
    public static void LoadLevelNext(LevelSelectData levelSelectData)
    {
        _levelToLoad = levelSelectData;
    }

    public static void LoadGame()
    {
        instance._LoadGame();
    }

    private void _LoadGame()
    {
        if (_SaveFileExists() == false) return;

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = _GetLevelDat();
        SaveFile save =  (SaveFile)bf.Deserialize(file);
        _LoadAllTowers(save);
        _LoadPlayer(save);
        file.Close();
    }

    private void _LoadPlayer(SaveFile save)
    {
        CurrencyManager.SetPlayerMoney(save.current_money);
        EnemyManager.SetWave_i(save.current_wave);
        Gate.SetFrontGateHealth(save._frontGateHealth);
    }

    private void _LoadAllTowers(SaveFile save)
    {
        foreach (var saved in save._saved_towers)
        {
            _LoadTower(saved);
        }
    }

    private void _LoadTower(_saved_tower saved)
    {
        Tower_SO dto = _TowerByString(saved.tower_dto);
        Vector3 pos = new Vector3(saved.x, saved.y, saved.z);
        Tower t = TowerSpawnManager.Instance.PlaceTowerSpecific(dto, pos, saved.current_health);
        t.transform.eulerAngles = new Vector3(0f, saved.euler_y, 0f);
        print($"Spawned {dto.name} at pos {pos}. It had {saved.current_health} hp");
    }

    public static void SaveGame()
    {
        instance._SaveGame();
    }

    private void _SaveGame()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = _GetLevelDat();
        SaveFile data = new SaveFile();
        _SaveTowers(data);
        _SavePlayer(data);
        bf.Serialize(file, data);
        file.Close();
    }

    private void _SavePlayer(SaveFile save)
    {
        save.current_wave = EnemyManager._public_wave_i;
        save.current_money = CurrencyManager.CurrentCash;
        save._frontGateHealth = Gate.FrontGateHealth;
    }

    private void _SaveTowers(SaveFile data)
    {
        var towers = TowerSpawnManager.GetAllTowers();

        foreach (var tower in towers)
        {
            _saved_tower tuple = new _saved_tower(tower);
            // print($"Saved tower {tuple.tower_dto}, at pos ({tuple.x}, {tuple.y}, {tuple.z})");
            data._saved_towers.Add(tuple);
        }
    }

    private bool _SaveFileExists()
    {
        if (levelSelectData == null) return false;
        return File.Exists(_path);
    }
    
    private FileStream _GetLevelDat()
    {
        if (levelSelectData == null) return null;
        FileStream file = File.Open(_path, FileMode.OpenOrCreate);
        return file;
    }
    
    /// <summary>
    /// Simple and safe Tower_SO getter
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    private Tower_SO _TowerByString(string str)
    {
        Tower_SO dto = null;
        foreach (var tower in TowerTypes)
        {
            if (tower.ToString() == str)
            {
                dto = tower;
                break;
            }
        }
        return dto;
    }
}
