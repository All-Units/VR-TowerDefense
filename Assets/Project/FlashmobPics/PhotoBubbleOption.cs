using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.GraphView;

[CustomEditor(typeof(PhotoBubbleOption))]
[CanEditMultipleObjects]
class PhotoBubbleEditor : Editor
{
    private PhotoBubbleOption p => ((PhotoBubbleOption)target);
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Resize"))
        {
            p.Resize();
        }
        base.OnInspectorGUI();
    }
}

public class PhotoBubbleOption : MonoBehaviour
{
    [SerializeField] Texture pictureToDisplay;
    
    [SerializeField] Transform bubbleParent;
    [SerializeField] MeshRenderer photoMesh;
    Material mat;
    // Start is called before the first frame update
    void Start()
    {
       
        Resize();
        
    }

    public void Resize()
    {
        
        if (Application.isEditor)
        {
            mat = photoMesh.sharedMaterial;
        }
        else
        {
            mat = photoMesh.material;
        }
            
        mat.mainTexture = pictureToDisplay;

        float h = (float)pictureToDisplay.height;
        float w = (float)pictureToDisplay.width;
        float ratio = Math.Min(h, w) / Math.Max(h, w);

        Vector3 scale = new Vector3(1f,1f,1f);
        //float radius = 0.5f * Mathf.Sqrt(1 + (ratio * ratio));
        //float zero_radius = 0.7071067811865475f;
        

        //If TALLER than WIDE, make X small (skinny)
        if (h > w){
            scale = new Vector3(ratio, 1f, 1f);
            bubbleParent.localScale = new Vector3(ratio, 1f, 1f);
        }

        //Else if WIDER than TALL, make Z small (portrait orientation)
        else if (w > h){
            scale = new Vector3(1f, 1f, ratio);
            bubbleParent.localScale = new Vector3(1f, ratio, 1f);
        }

        photoMesh.transform.localScale = scale;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
