using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class TrailerCamMover : MonoBehaviour
{
    public GameObject playerPref;

    public float MoveDelay = 0f;

    public float moveSpeed;
    public Vector3 moveDir = new Vector3(1f, 0f, 0f);
    public Vector3 rotateDir = Vector3.zero;
    public float rotateTarget = 0f;
    public bool isMoving = false;

    public List<ProjectileTower> CannonBattery = new List<ProjectileTower>();
    public float _DelayBeforeFiring = 1f;
    public float _DelayBetweenShots = 1f;

    public List<GameObject> towerGroupsToSpawn = new List<GameObject>();
    public float _DelayBeforeFirstSpawn = 1f;
    public float _TimeBetweenGroups = 0.2f;
    // Start is called before the first frame update
    void Start()
    {
        if (playerPref == null)
            playerPref = FindObjectOfType<InventoryManager>().gameObject;
        if (playerPref.activeInHierarchy)
        {
            gameObject.SetActive(false);
            return;
        }
        StartCoroutine(_DelayMove());
        StartCoroutine(_FireBattery());
        StartCoroutine(_EnableTowerGroups());
        
    }
    IEnumerator _EnableTowerGroups()
    {
        foreach (var group in towerGroupsToSpawn)
            group.SetActive(false);
        yield return new WaitForSeconds(_DelayBeforeFirstSpawn);

        foreach (var group in towerGroupsToSpawn)
        {
            group.SetActive(true);
            yield return new WaitForSeconds(_TimeBetweenGroups);
        }
    }
    void _SetGroupTo(_TowerGroup group, bool active)
    {
        foreach (var tower in group.towers)
            tower.SetActive(active);
    }
    IEnumerator _FireBattery()
    {
        yield return new WaitForSeconds(_DelayBeforeFiring);
        while (true)
        {
            foreach (var tower in CannonBattery)
            {
                tower.FireOverride();
                yield return new WaitForSeconds(_DelayBetweenShots);
            }
            yield return new WaitForSeconds(2f);
        }
        
    }
    IEnumerator _DelayMove()
    {
        if (isMoving == false) yield break;

        isMoving = false;
        yield return new WaitForSeconds(MoveDelay);
        isMoving = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isMoving = !isMoving;
        } 
        if (isMoving)
        {
            Vector3 pos = transform.localPosition;
            pos += (moveDir * moveSpeed * Time.deltaTime);
            transform.localPosition = pos;

            Vector3 euler = transform.eulerAngles;
            euler += rotateDir * Time.deltaTime;
            if (euler.x <= rotateTarget)
            {
                euler.x = rotateTarget;
                isMoving = false;
            }
            transform.eulerAngles = euler;
            
        }
    }
    [System.Serializable]
    public struct _TowerGroup
    {
        public List<GameObject> towers;
    }
}
