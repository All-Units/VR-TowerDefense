﻿using UnityEngine;

[CreateAssetMenu(menuName = "SO/Level Select Data", fileName = "New Level Data")]
public class LevelSelectData : ScriptableObject
{
    public int sceneToLoad;
    public string sceneName;
    public string title;
    public Material OverrideTowerColor;
}