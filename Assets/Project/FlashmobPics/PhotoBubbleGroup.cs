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
    int ROW_SIZE = 3;
    private PhotoBubbleGroup p => ((PhotoBubbleGroup)target);
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Populate MediaSO list"))
        {
            for (int i = 0; i < p.media.Count; i++)
            {
                if (p.media[i] == null)
                {
                    p.media[i] = new MediaSO();
                }
            }
        }
        if (GUILayout.Button("Place All"))
        {
            p.PlaceAllBubbles();
        }
        base.OnInspectorGUI();
        EditorGUILayout.BeginHorizontal();
        int row = 0;
        
        foreach (MediaSO media in p.media)
        {
            if (media == null)
                continue;
            media.UpdateName();
            if (media.PreviewIcon == null)
                continue;
            GUILayout.Label(media.PreviewIcon);
            row++;
            if (row >= ROW_SIZE)
            {
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                row = 0;
            }
            
            
        }
        EditorGUILayout.EndHorizontal();
    }
}

public class PhotoBubbleGroup : MonoBehaviour
{
    public List<MediaSO> media = new List<MediaSO>();
    [SerializeField] List<Texture> pictures = new List<Texture>();
    [SerializeField] GameObject bubblePrefab;

    [SerializeField] float radius = 2f;
    List<PhotoBubbleOption> bubbles = new List<PhotoBubbleOption>();
    // Start is called before the first frame update
    void Start()
    {
       bubbles = GetComponentsInChildren<PhotoBubbleOption>().ToList();
       foreach (PhotoBubbleOption b in bubbles)
        {
            if (b.IS_CENTER)
                centerBubble = b;
        }
        if (centerBubble == null)
            Debug.LogError("WHY NO CENTER BUBBLE WTFFFFF");

       
    }

   
    // Update is called once per frame
    void Update()
    {
        
    }
    public void SetCenterPicture(MediaSO media)
    {
        if (centerBubble == null)
        {
            Debug.LogError("No center bubble???", gameObject);
            return;
        }
        centerBubble.media = media;
        centerBubble.Resize();
    }

    PhotoBubbleOption centerBubble;
    public void PlaceAllBubbles()
    {
        transform.DestroyChildren();
        float i = 0;
        GameObject bubble;
        PhotoBubbleOption option;
        foreach (MediaSO t in media)
        {
            i += 1;
            bubble = (GameObject)PrefabUtility.InstantiatePrefab(bubblePrefab, transform);
            option = bubble.GetComponent<PhotoBubbleOption>();
            float angle = i / (float)media.Count;
            angle *= 360f;
            
            option.PlacePhoto(t, radius, angle);
            option.parentGroup = this;
        }
        bubble = (GameObject)PrefabUtility.InstantiatePrefab(bubblePrefab, transform);
        option = bubble.GetComponent<PhotoBubbleOption>();
        option.PlacePhoto(media.Last(), 0f, 0f);
        bubble.transform.localScale *= 2f;
        centerBubble = option;
        centerBubble.IS_CENTER = true;
    }
}
