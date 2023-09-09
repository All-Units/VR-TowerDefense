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
    }
