using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using System.Linq;

[CustomEditor(typeof(PhotoBubbleGroup))]
[CanEditMultipleObjects]
class PhotoGroupEditor : Editor
{
    private PhotoBubbleGroup p => ((PhotoBubbleGroup)target);
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Place All"))
        {
            p.PlaceAllBubbles();
        }
        base.OnInspectorGUI();
    }
}

public class PhotoBubbleGroup : MonoBehaviour
{
    [SerializeField] List<Texture> pictures = new List<Texture>();
    [SerializeField] GameObject bubblePrefab;

    [SerializeField] float radius = 2f;
    // Start is called before the first frame update
    void Start()
    {
       
       
    }

   
    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlaceAllBubbles()
    {
        transform.DestroyChildren();
        float i = 0;
        GameObject bubble;
        PhotoBubbleOption option;
        foreach (Texture t in pictures)
        {
            i += 1;
            bubble = (GameObject)PrefabUtility.InstantiatePrefab(bubblePrefab, transform);
            option = bubble.GetComponent<PhotoBubbleOption>();
            float angle = i / (float)pictures.Count;
            angle *= 360f;
            
            option.PlacePhoto(t, radius, angle);
        }
        bubble = (GameObject)PrefabUtility.InstantiatePrefab(bubblePrefab, transform);
        option = bubble.GetComponent<PhotoBubbleOption>();
        option.PlacePhoto(pictures.Last(), 0f, 0f);
        bubble.transform.localScale *= 2f;

    }
}
