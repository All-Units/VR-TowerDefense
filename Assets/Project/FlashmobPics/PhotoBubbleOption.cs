using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.Video;

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
    
    [SerializeField] MediaSO mediaToDisplay;
    
    [SerializeField] Transform bubbleParent;
    [SerializeField] MeshRenderer photoMesh;
    [SerializeField] Transform displayParent;
    Material mat;
    VideoPlayer videoPlayer;

    [SerializeField] Material blankVideoTexture;
    // Start is called before the first frame update
    void Start()
    {
       
        Resize();
        
    }

    public void Resize()
    {
        

        if (Application.isEditor && false)
        {
            mat = photoMesh.sharedMaterial;
        }
        else
        {
            mat = photoMesh.material;
        }
        if (mediaToDisplay.video != null)
        {
            photoMesh.material = blankVideoTexture;
        }


        if (mediaToDisplay.picture)   
            mat.mainTexture = mediaToDisplay.picture;
        else if (mediaToDisplay.video)
        {
            if (photoMesh.GetComponent<VideoPlayer>() == null)
                videoPlayer = photoMesh.gameObject.AddComponent<VideoPlayer>();
            videoPlayer.clip = mediaToDisplay.video;
            videoPlayer.playOnAwake = true;
            videoPlayer.isLooping = true;
            // Direct if uses sound, otherwise nothing
            videoPlayer.audioOutputMode = mediaToDisplay.usesSound ?
                VideoAudioOutputMode.Direct : VideoAudioOutputMode.None;
        }

        float h = (float)mediaToDisplay.height;
        float w = (float)mediaToDisplay.width;
        float ratio = Math.Min(h, w) / Math.Max(h, w);

        Vector3 scale = new Vector3(1f,1f,1f);
        
        
        bubbleParent.localScale = new Vector3(1f, 1f, 1f);
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

    public void PlacePhoto(MediaSO media, float radius, float angle)
    {
        mediaToDisplay = media;
        Resize();

        displayParent.localPosition = new Vector3(0f, radius, 0f);
        transform.localEulerAngles = new Vector3(0f, 0f, angle);

        displayParent.localEulerAngles = new Vector3(1f, 1f, 360f - angle);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
