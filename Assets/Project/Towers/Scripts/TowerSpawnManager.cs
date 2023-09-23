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
        public static bool CouldAffordCurrentTower => CurrencyManager.CouldAfford(Instance.currentTower);
        public static TowerSpawnManager Instance;
        private Tower_SO currentTower;

        private void Awake()
        {
            Instance = this;
        }

        public static void RefreshGhost()
        {
            if (lastPos.y < -100000 || _isGhostOpen())
                return;
            Instance.PlaceGhost(lastPos);
        }

        private static bool _isGhostOpen()
        {
            foreach (Transform t in Instance.ghostsRoot)
            {
                if (t.gameObject.activeInHierarchy) return true;
            }
            return false;
        }

        private static Vector3 lastPos = Vector3.negativeInfinity;
        public void PlaceGhost(Vector3 targetPos)
        {
            if (ghostObjects.ContainsKey(currentTower) == false)
            {
                ghostObjects.Add(currentTower, Instantiate(currentTower.ghostObject, ghostsRoot));
            }
            
            ghostObjects[currentTower].transform.position = targetPos;
            lastPos = targetPos;
            if(ghostObjects[currentTower].activeSelf == false)
                ghostObjects[currentTower].SetActive(true);
        }

        public void HideGhost()
        {
            foreach (Transform tower in ghostsRoot)
                tower.gameObject.SetActive(false);
            
        }

        public static Dictionary<Vector3, Tower> _towersByPos = new Dictionary<Vector3, Tower>();
        public void PlaceTower(Vector3 targetPos)
        {
            if (CurrencyManager.TryCanAfford(currentTower) == false)
                return;
            if (_towersByPos.ContainsKey(targetPos)) return;
            var tower = Instantiate(currentTower.towerPrefab, targetPos, Quaternion.identity);
            tower.transform.SetParent(towersRoot);
            
            // Todo refactor needed
            Tower t = tower.GetComponentInChildren<Tower>(); 
            Vector3 pos = t.transform.position;
            _towersByPos.Add(pos, t);
            Minimap.instance.SpawnTowerAt(pos, currentTower);
            // End refactor needed
            
            HideGhost();
        }        
        
        public void PlaceTowerSpecific(Tower_SO targetTower, Vector3 targetPos)
        {
            var tower = Instantiate(targetTower.towerPrefab, targetPos, Quaternion.identity);
            tower.transform.SetParent(towersRoot);
            
            // Todo refactor needed
            Tower t = tower.GetComponentInChildren<Tower>(); 
            Vector3 pos = t.transform.position;
            _towersByPos.Add(pos, t);
            Minimap.instance.SpawnTowerAt(pos, currentTower);
            // End refactor needed
        }

        public static void SetTower(Tower_SO towerDTO)
        {
            Instance.HideGhost();
            Instance.currentTower = towerDTO;
        }

        public static void UpgradeTower(Tower towerToUpgrade, Tower_SO upgrade)
        {
            if (Instance)
                Instance._UpgradeTower(towerToUpgrade, upgrade);
        }

        private void _UpgradeTower(Tower towerToUpgrade, Tower_SO upgrade)
        {
            var pos = towerToUpgrade.transform.position;
            RemoveTower(towerToUpgrade);
            PlaceTowerSpecific(upgrade, pos);
        }

        private void RemoveTower(Tower towerToRemove)
        {
            Tower t = towerToRemove.GetComponentInChildren<Tower>(); 
            Vector3 pos = t.transform.position;
            _towersByPos.Remove(pos);
            Destroy(towerToRemove.gameObject);
        }
    }
}
