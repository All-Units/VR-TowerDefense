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

        public string name;
        public string description;

        [SerializeField] private List<TowerUpgrade> upgrades = new List<TowerUpgrade>();

        public List<TowerUpgrade> GetUpgrades() => upgrades;
    }

[Serializable]
public struct TowerUpgrade
{
    public Tower_SO upgrade;
    public int cost;
}
