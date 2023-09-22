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

        public static Dictionary<Vector3, Tower> _towersByPos = new Dictionary<Vector3, Tower>();
        public void PlaceTower(Vector3 targetPos)
        {
            targetPos.y += yOffset;
            print($"Started placing tower");
            if (CurrencyManager.CanAfford(currentTower) == false)
            {
                print($"Returning because POOR");
                return;
            }
            if (_towersByPos.ContainsKey(targetPos))
            {
                print($"Returning because contained in dict already");
                return;
            }
            var tower = Instantiate(currentTower.towerPrefab, targetPos, Quaternion.identity);
            tower.transform.SetParent(towersRoot);
            Tower t = tower.GetComponentInChildren<Tower>();
            Vector3 pos = t.transform.position;
            print($"Spawned tower");
            if (_towersByPos.ContainsKey(pos))
            {
                print($"Already a tower at {pos}");
                t.removeFromDict = false;
                Destroy(tower.gameObject);
                HideGhost();
                return;
            }
            _towersByPos.Add(pos, t);
            //Minimap.instance.SpawnTowerAt(pos, currentTower);
            CurrencyManager.PayFor(currentTower.cost);
            HideGhost();
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
    }
}
