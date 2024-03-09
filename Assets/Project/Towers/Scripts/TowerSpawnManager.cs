using System;
using System.Collections.Generic;
using Unity.Collections;
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

        public static bool CouldAffordCurrentTower => CurrencyManager.CanAfford(Instance.currentTower.cost);
        public static TowerSpawnManager Instance;
        public static Action<Tower_SO> OnTowerSpawned;
        private Tower_SO currentTower;
        public static Tower_SO GetCurrentTower => Instance ? Instance.currentTower : null;

        public float yOffset = -0.1f;
        private void Awake()
        {
            Instance = this;
        }

        public static void RefreshGhost()
        {
            if (lastPos.y < -100000 || _isGhostOpen())
                return;
            Instance.PlaceGhost(lastPos, lastPos);
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
        public void PlaceGhost(Vector3 targetPos, Vector3 fromPos)
        {
            if (ghostObjects.ContainsKey(currentTower) == false)
            {
                ghostObjects.Add(currentTower, Instantiate(currentTower.ghostObject, ghostsRoot));
                TowerRangeVisualization rangeVZ = ghostObjects[currentTower].GetComponentInChildren<TowerRangeVisualization>();
                if (rangeVZ != null && currentTower is ProjectileTower_SO projectile)
                {
                    rangeVZ.SetStats(projectile);
                }
            }
            
            ghostObjects[currentTower].transform.position = targetPos;
            var rotation = ghostObjects[currentTower].transform.rotation;
            ghostObjects[currentTower].transform.LookAt(new Vector3(fromPos.x, targetPos.y, fromPos.z));
            lastPos = targetPos;
            if(ghostObjects[currentTower].activeSelf == false)
                ghostObjects[currentTower].SetActive(true);
        }
        public static void HideGhosts()
        {
            if (Instance == null)return;
            Instance.HideGhost();
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
            if (Instance == null) return;
            Instance.deathSounds.PlayClipAt(pos);
        }

        public static Dictionary<Vector3, Tower> _towersByPos = new Dictionary<Vector3, Tower>();
        public void PlaceTower(Vector3 targetPos)
        {
            targetPos.y += yOffset;
            //print($"Started placing tower");
            if (CurrencyManager.CanAfford(currentTower.cost) == false)
            {
                return;
            }
            if (_towersByPos.ContainsKey(targetPos))
            {
                return;
            }

            var targetTransform = ghostObjects[currentTower].transform;
            
            var tower = Instantiate(currentTower.towerPrefab, targetTransform.position, targetTransform.rotation);
            tower.transform.SetParent(towersRoot);
            tower.SpawnTower();
            
            OnTowerSpawned?.Invoke(currentTower);
            
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
            CurrencyManager.TakeFromPlayer(currentTower.cost);
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
            if (Instance)
            {
                Instance.HideGhost();
                Instance.currentTower = towerDTO;
                OnTowerSet.Invoke();
            }
            else
            {
                Debug.LogWarning("No Tower Manager Present!!!");
            }
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
            //print($"Destroying {towerToRemove.gameObject.name}");
            Destroy(towerToRemove.gameObject);
        }

        public static void SellTower(Tower tower)
        {
            if(Instance == null) return;
            
            Instance.RemoveTower(tower);
            CurrencyManager.GiveToPlayer(tower.dto.cost/2);
        }

        public static IEnumerable<Tower> GetAllSpawnedTowers()
        {
            return Instance ? Instance.GetComponentsInChildren<Tower>() : Array.Empty<Tower>();
        }
    }
}
