using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Project.Towers.Scripts
{
    public class TowerSpawnManager : MonoBehaviour
    {
        private Dictionary<Tower_SO, GameObject> ghostObjects = new Dictionary<Tower_SO, GameObject>();
        [SerializeField] private Transform ghostsRoot;
        [SerializeField] private Transform towersRoot;
        [SerializeField] private AudioClipController placingSounds;
        [SerializeField] private AudioClipController deathSounds;

        public static bool CouldAffordCurrentTower => CurrencyManager.CouldAfford(Instance.currentTower);
        public static TowerSpawnManager Instance;
        private Tower_SO currentTower;
        public static Tower_SO GetCurrentTower => Instance.currentTower;

        public float yOffset = -0.1f;
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

        /// <summary>
        /// TODO: REFACTOR
        /// I KNOW THIS IS BAD BUT IT'S LATE
        /// </summary>
        public static void PlayDeathSounds(Vector3 pos)
        {
            Instance.deathSounds.PlayClipAt(pos);
        }

        public static Dictionary<Vector3, Tower> _towersByPos = new Dictionary<Vector3, Tower>();
        public void PlaceTower(Vector3 targetPos)
        {
            targetPos.y += yOffset;
            //print($"Started placing tower");
            if (CurrencyManager.CanAfford(currentTower) == false)
            {
                return;
            }
            if (_towersByPos.ContainsKey(targetPos))
            {
                return;
            }
            var tower = Instantiate(currentTower.towerPrefab, targetPos, Quaternion.identity);
            tower.transform.SetParent(towersRoot);
            tower.SpawnTower();
            
            // Todo refactor needed
            Tower t = tower.GetComponentInChildren<Tower>(); 
            Vector3 pos = t.transform.position;
            if (_towersByPos.ContainsKey(pos))
            {
                t.removeFromDict = false;
                Destroy(tower.gameObject);
                HideGhost();
                return;
            }
            _towersByPos.Add(pos, t);
            Minimap.SpawnTower(pos, currentTower);
            // Todo: Refactor to use the Tower.OnTowerSpawn event
            placingSounds.PlayClipAt(pos);
            //Minimap.instance.SpawnTowerAt(pos, currentTower);
            // End refactor needed
            
            //Minimap.instance.SpawnTowerAt(pos, currentTower);
            CurrencyManager.PayFor(currentTower.cost);
            HideGhost();
        }        
        
        public Tower PlaceTowerSpecific(Tower_SO targetTower, Vector3 targetPos)
        {
            var tower = Instantiate(targetTower.towerPrefab, targetPos, Quaternion.identity);
            tower.transform.SetParent(towersRoot);
            
            // Todo refactor needed
            Tower t = tower.GetComponentInChildren<Tower>(); 
            Vector3 pos = t.transform.position;
            _towersByPos.Add(pos, t);
            
            // Todo: Refactor to use the Tower.OnTowerSpawn event
            //Minimap.instance.SpawnTowerAt(pos, currentTower);
            // End refactor needed

            tower.SpawnTower();
            return tower;
        }

        public static void ClearAllTowers()
        {
            foreach (var t in _towersByPos)
            {
                Destroy(t.Value.gameObject);
            }

            _towersByPos = new Dictionary<Vector3, Tower>();
        }

        public static UnityEvent OnTowerSet = new UnityEvent();
        public static void SetTower(Tower_SO towerDTO)
        {
            Instance.HideGhost();
            Instance.currentTower = towerDTO;
            OnTowerSet.Invoke();
        }

        public static Tower UpgradeTower(Tower towerToUpgrade, Tower_SO upgrade)
        {
            if (Instance)
                return Instance._UpgradeTower(towerToUpgrade, upgrade);
            return null;
        }

        private Tower _UpgradeTower(Tower towerToUpgrade, Tower_SO upgrade)
        {
            var pos = towerToUpgrade.transform.position;
            RemoveTower(towerToUpgrade);
            return PlaceTowerSpecific(upgrade, pos);
        }

        private void RemoveTower(Tower towerToRemove)
        {
            Tower t = towerToRemove.GetComponentInChildren<Tower>(); 
            Vector3 pos = t.transform.position;
            _towersByPos.Remove(pos);
            Destroy(towerToRemove.gameObject);
        }

        public static void SellTower(Tower tower)
        {
            if(Instance == null) return;
            
            Instance.RemoveTower(tower);
        }
    }
}
