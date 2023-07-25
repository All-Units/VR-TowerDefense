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

        private void Start()
        {
            Instance = this;
        }

        public void PlaceGhost(Tower_SO towerDTO, Vector3 targetPos)
        {
            if (ghostObjects.ContainsKey(towerDTO) == false)
            {
                ghostObjects.Add(towerDTO, Instantiate(towerDTO.ghostObject, ghostsRoot));
            }
            
            ghostObjects[towerDTO].transform.position = targetPos;
            if(ghostObjects[towerDTO].activeSelf == false)
                ghostObjects[towerDTO].SetActive(true);
        }

        public void HideGhost(Tower_SO towerDTO)
        {
            if (ghostObjects.TryGetValue(towerDTO, out var ghost))
            {
                ghost.SetActive(false);
            }
        }

        public void PlaceTower(Tower_SO towerDTO, Vector3 targetPos)
        {
            var tower = Instantiate(towerDTO.towerPrefab, targetPos, Quaternion.identity);
            tower.transform.SetParent(towersRoot);
            
            HideGhost(towerDTO);
        }
    }
}
