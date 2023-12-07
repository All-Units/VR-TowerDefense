using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Tower", fileName = "New Tower")]
    public class Tower_SO : ScriptableObject
    {
        public GameObject ghostObject;
        public Tower towerPrefab;
        public GameObject iconPrefab;
        public GameObject minimapPrefab;
        public int cost;

        public new string name;
        public string description;
        public TowerStats stats;
        public PlayerItem_SO playerItem_SO;

        [SerializeField] private List<TowerUpgrade> upgrades = new();

        public List<TowerUpgrade> GetUpgrades() => upgrades;
    }

[Serializable]
public struct TowerUpgrade
{
    public Tower_SO upgrade;
    public int cost;
}
