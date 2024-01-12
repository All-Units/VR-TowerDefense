using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;

public class TrailerCamMover : MonoBehaviour
{
    public List<GameObject> towerList = new List<GameObject>();
    public float towerTimeLapseRate = 0.4f;
    public GameObject playerPref;
    public float moveSpeed;
    public Vector3 moveDir = new Vector3(1f, 0f, 0f);
    public bool isMoving = false;
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
        StartCoroutine(_TowerTimeLapse());
        
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
            Vector3 pos = transform.position;
            pos += (moveDir * moveSpeed * Time.deltaTime);
            transform.position = pos;
        }
    }
    IEnumerator _TowerTimeLapse()
    {
        LinkedList<GameObject> inactive = new LinkedList<GameObject>();
        foreach (GameObject tower in towerList)
        {
            tower.SetActive(false );
            inactive.AddLast(tower);
        }
        while (inactive.Count > 0)
        {
            GameObject tower = inactive.First.Value;
            tower.SetActive(true);
            inactive.RemoveFirst();
            yield return new WaitForSeconds(towerTimeLapseRate);
        }
    }
}
