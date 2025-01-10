using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class TrailerCamMover : MonoBehaviour
{
    public GameObject playerPref;
    public float moveSpeed;
    public float moveDelay = 0f;
    public Vector3 moveDir = new Vector3(1f, 0f, 0f);
    public Vector3 rotateDir = new Vector3(-43f, 0f, 0f);
    public float rotateTarget = 0f;
    public float fovDelta = 0f;
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
        cam = GetComponent<Camera>();
        StartCoroutine(_DelayMove());
        
    }
    IEnumerator _DelayMove()
    {
        if (isMoving == false || moveDelay == 0f) yield break;
        isMoving = false;
        yield return new WaitForSeconds(moveDelay);
        isMoving = true;
    }
    Camera cam;

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
            if (euler.x <= rotateTarget)
            {
                euler.x = rotateTarget;
                isMoving = false;
                return;
            }
            transform.eulerAngles = euler;

            if (cam != null)
            {
                cam.fieldOfView += fovDelta * Time.deltaTime;
            }
        }
    }
}
