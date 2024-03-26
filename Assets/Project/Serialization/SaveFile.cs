using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

[System.Serializable]
public class SaveFile
{
    public int _frontGateHealth;
    public int current_money;
    public int current_wave;
    public List<_saved_tower> _saved_towers = new List<_saved_tower>();
    
}
[System.Serializable]
public struct _saved_tower
{

    public string tower_dto;
    //public Vector3 position;
    public float x;
    public float y;
    public float z;
    public float euler_y;
    public int current_health;
    /*
    public _saved_tower(Tower_SO dto, Vector3 pos, int health)
    {
        this.tower_dto = dto;
        this.position = pos;
        this.current_health = health;
    }*/
    public _saved_tower(Tower t)
    {
        this.tower_dto = t.dto.ToString();
        Vector3 pos = t.transform.position;
        this.x = pos.x;
        this.y = pos.y;
        this.z = pos.z;
        euler_y = t.transform.eulerAngles.y;
        this.current_health = t.healthController.CurrentHealth;
    }
}
