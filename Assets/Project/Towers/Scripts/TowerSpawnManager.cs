using System;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Towers.Scripts
{
    public class TowerSpawnManager : MonoBehaviour
    {
        private Dictionary<Tower_SO, GameObject> ghostObjects = new Dictionary<Tower_SO, GameObject>();
        [SerializeField] private Transform ghostsRoot;
        [SerializeField] private Transform towersRoot;

        public static TowerSpawnManager Instance;
        private Tower_SO currentTower;

        private void Awake()
        {
            Instance = this;
        }

        public void PlaceGhost(Vector3 targetPos)
        {
            if (ghostObjects.ContainsKey(currentTower) == false)
            {
                ghostObjects.Add(currentTower, Instantiate(currentTower.ghostObject, ghostsRoot));
            }
            
            ghostObjects[currentTower].transform.position = targetPos;
            if(ghostObjects[currentTower].activeSelf == false)
                ghostObjects[currentTower].SetActive(true);
        }

        public void HideGhost()
        {
            foreach (Transform tower in ghostsRoot)
                tower.gameObject.SetActive(false);
            
        }

        public void PlaceTower(Vector3 targetPos)
        {
            //print("Instantiating tower");
            var tower = Instantiate(currentTower.towerPrefab, targetPos, Quaternion.identity);
            tower.transform.SetParent(towersRoot);
            
            HideGhost();
        }

        public static void SetTower(Tower_SO towerDTO)
        {
            Instance.HideGhost();
            Instance.currentTower = towerDTO;
            print($"Set current tower to {towerDTO.name}");
        }
    }
}
