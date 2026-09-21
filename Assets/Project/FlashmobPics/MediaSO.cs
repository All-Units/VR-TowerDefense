using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Video;
/*
[CustomPropertyDrawer(typeof(MediaSO))]
public class MediaDrawer : PropertyDrawer
{
    // Draw the property inside the given rect
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        //MediaSO target = (MediaSO)property.serializedObject.targetObject;
        //EditorGUI.DrawPreviewTexture(position, target.PreviewIcon);
        
        // First get the attribute since it contains the range for the slider
        RangeAttribute range = attribute as RangeAttribute;

        // Now draw the property as a Slider or an IntSlider based on whether it's a float or integer.
        if (property.propertyType == SerializedPropertyType.Float)
            EditorGUI.Slider(position, property, range.min, range.max, label);
        else if (property.propertyType == SerializedPropertyType.Integer)
            EditorGUI.IntSlider(position, property, Convert.ToInt32(range.min), Convert.ToInt32(range.max), label);
        else
            EditorGUI.LabelField(position, label.text, "Use Range with float or int.");
            
        base.OnGUI(position, property, label);
    }
}*/

[CustomEditor(typeof(MediaSO))]
public class MediaSOEditor : Editor
{
    MediaSO m => ((MediaSO)target);
    double _lastErrorTime = 0;
    public override void OnInspectorGUI()
    {
        bool waitedEnough = (EditorApplication.timeSinceStartup - _lastErrorTime > 2);
        if (m.picture && m.video)
        {
            _lastErrorTime = EditorApplication.timeSinceStartup;
            Debug.LogError("Cannot have both a movie and a picture!", m);
        }
        
        if (GUILayout.Button("Clear"))
        {
            Undo.RecordObject(m, "Clear");
            m.video = null;
            m.picture = null;
            m.usesSound = false;
        }
        

        base.OnInspectorGUI();
    }
    public override Texture2D RenderStaticPreview(string assetPath, UnityEngine.Object[] subAssets, int width, int height)
    {

        if (m == null || m.PreviewIcon == null)
            return null;

        // example.PreviewIcon must be a supported format: ARGB32, RGBA32, RGB24,
        // Alpha8 or one of float formats
        Texture2D tex = new Texture2D (width, height);
        EditorUtility.CopySerialized (m.PreviewIcon, tex);
        return tex;
    }
}

[Serializable]
public class MediaSO : ScriptableObject
{
    [HideInInspector] public Texture2D PreviewIcon
    {
        get
        {
            Texture2D t = null;
            if (picture)
            {
                t = AssetPreview.GetAssetPreview(picture);
            }
            else if (video)
            {
                t = AssetPreview.GetMiniThumbnail(video);
            }
            return t;
        }
    }
    public int height
    {
        get
        {
            if (picture)
                return picture.height;
            else if (video)
                return Convert.ToInt32(video.height);
            return -1;
        }
    }
    public int width
    {
        get
        {
            if (picture)
                return picture.width;
            else if (video)
                return Convert.ToInt32(video.width);
            return -1;
        }
    }
    public void UpdateName()
    {
        if (picture)
        {
            name = picture.name;
        }
        else if (video)
        {
            name = video.name;
        }
        else
        {
            name = "(empty)";
        }
    }


    [SerializeField] public Texture picture;
    [SerializeField] public VideoClip video;
    /// <summary>
    /// Whether or not the audio clip plays its sound (most are mute)
    /// </summary>
    [SerializeField] public bool usesSound = false;

}
