using System.Collections;
using System.Collections.Generic;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(SpiralStaircaseGenerator))]
class SpiralStaircaseEditor : Editor
{
    private SpiralStaircaseGenerator ssg => (SpiralStaircaseGenerator)target;
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Generate Staircase"))
        {
            ssg.GenerateStaircase();
        }
        base.OnInspectorGUI();
    }
}
#endif

public class SpiralStaircaseGenerator : MonoBehaviour
{
    public GameObject staircasePrefab;
    public float staircaseHeight = 0.5f;
    public float angle = 12f;
    public float distanceFromCenter = 10f;
    public int stairCount = 20;
    public void GenerateStaircase()
    {
        transform.DestroyChildrenImmediate();
        Vector3 center = transform.position;
        float current_angle = 0f;
        for (int i = 0; i < stairCount; i++)
        {
            GameObject stair = Instantiate(staircasePrefab, transform);
            stair.transform.position = center;
            stair.transform.GetChild(0).localPosition = new Vector3(distanceFromCenter, 0f, 0f);
            center.y += staircaseHeight;
            stair.transform.localEulerAngles = new Vector3(0f, current_angle, 0f);
            current_angle += angle;
            stair.name = $"Step {i}";
        }
        print("Staircase generated");
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
