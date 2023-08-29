using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    public Material a;
    public Material b;
    public GameObject cubePrefab;
    public int dimensions;
    // Start is called before the first frame update
    void Start()
    {
        _genGrid();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void _genGrid()
    {
        for (int i = 0; i < dimensions; i++)
        {
            for (int j = 0; j < dimensions; j++)
            {
                GameObject cube = Instantiate(cubePrefab, transform);
                cube.transform.localPosition = new Vector3(i, 0f, j);
                //i being even must match j being even
                bool isA = ((i % 2 == 0) == (j % 2 == 0));
                Material mat = isA ? a : b ;
                cube.GetComponent<MeshRenderer>().material = mat;

                cube.name = $"{mat.name} : ({i}, {j})"; 
            }
        }
    }
}
