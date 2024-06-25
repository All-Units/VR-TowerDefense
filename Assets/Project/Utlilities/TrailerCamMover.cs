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
    public bool isMoving = false;
    // Start is called before the first frame update
    void Start()
    {
        if (playerPref == null)
            playerPref = FindObjectOfType<InventoryManager>().gameObject;
        if (playerPref.activeInHierarchy)
        {
            gameObject.SetActive(false);
        }
        StartCoroutine(_DelayMove());
        
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
            Vector3 pos = transform.position;
            pos += (moveDir * moveSpeed * Time.deltaTime);
            transform.position = pos;

            Vector3 euler = transform.eulerAngles;
            euler += rotateDir * Time.deltaTime;
            transform.eulerAngles = euler;
        }
    }
}
