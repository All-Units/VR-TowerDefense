using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrailerCamMover : MonoBehaviour
{
    public float moveSpeed;
    public Vector3 moveDir = new Vector3(1f, 0f, 0f);
    public bool isMoving = false;
    // Start is called before the first frame update
    void Start()
    {
        
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
}
