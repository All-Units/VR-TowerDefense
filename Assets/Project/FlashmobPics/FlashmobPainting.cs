using System.Collections;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Video;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(FlashmobPainting))]
public class PaintingEditor : Editor
{
    int ROW_SIZE = 3;
    FlashmobPainting p => (FlashmobPainting)target;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Populate painting list"))
        {
            // Init paintings list with a dozen empty MediaSOs
            if (p.paintingsList.Count == 0 || p.paintingsList.First() == null)
            {
                p.paintingsList.Clear();
                for (int _i = 0; _i < 12; _i++)
                {
                    MediaSO m = new MediaSO();
                    EditorUtility.SetDirty(m);
                    p.paintingsList.Add(m);
                }
                EditorUtility.SetDirty(p.gameObject);
            }
        }
        if (p && p.HasPainting)
        {
            GUILayout.Label(p.previewIcon);
        }
        else if (p && p.HasPainting == false)
        {
            GUILayout.Label("(NO MEDIA)");
        }
        int row = 0;
        GUILayout.Label("\n\n>>> PAINTINGS MASTER LIST: <<<");
        EditorGUILayout.BeginHorizontal();
        foreach (MediaSO media in p.paintingsList)
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


[ExecuteInEditMode]
public class FlashmobPainting : MonoBehaviour
{
    [SerializeField] MediaSO media;
    public bool HasPainting => (media != null && previewIcon != null);
    public Texture2D previewIcon => media.PreviewIcon;

    [SerializeField] Transform paintingParent;
    [SerializeField] MeshRenderer paintingRenderer;
    [SerializeField] Material blankVideoTexture;
    [SerializeField] Material basePhotoTexture;

    [Tooltip("Multiplies the paintings local scale by a float with these bounds, inclusive")]
    [SerializeField] Vector2 paintingSizeBounds = new Vector2(1f, 3f);

    [SerializeField] public List<MediaSO> paintingsList = new List<MediaSO>();

    VideoPlayer videoPlayer
    {
        get
        {
            if (_player == null)
            {
                _player = paintingRenderer.GetComponent<VideoPlayer>();
                if (_player == null)
                    _player = paintingRenderer.gameObject.AddComponent<VideoPlayer>();
            }
            return _player;
        }
    }
    VideoPlayer _player;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(_initMedia());
        if (Application.isPlaying)
        {
            print("Running because play mode");
            Resize();

            return;
        }
        print("not running because editor!");
    }
    public void Resize()
    {
        
        Vector3 ls = transform.localScale;
        transform.localScale *= UnityEngine.Random.Range(paintingSizeBounds.x, paintingSizeBounds.y);
        print($"Scale was: {ls}, is now {transform.localScale}");

        if (media.HasMedia == false)
            return;
        if (media.video != null)
        {
            paintingRenderer.material = blankVideoTexture;
        }
        else if (media.picture != null)
        {
            paintingRenderer.material = basePhotoTexture;
        }
        Material mat = paintingRenderer.material;
        


        if (media.picture)
        {
            mat.mainTexture = media.picture;
            videoPlayer.enabled = false;
            videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
            videoPlayer.Stop();
            Destroy(_player);
            IEnumerator _setNull()
            {
                yield return new WaitForEndOfFrame();
                _player = null;
            }
            StartCoroutine(_setNull());
        }
        else if (media.video)
        {
            Destroy(_player);
            IEnumerator _setNull()
            {
                yield return new WaitForEndOfFrame();
                _player = null;
                videoPlayer.enabled = true;
                videoPlayer.clip = media.video;
                videoPlayer.playOnAwake = true;
                videoPlayer.isLooping = true;
                videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
                videoPlayer.Play();
            }
            StartCoroutine(_setNull());
            
            
        }

        float h = (float)media.height;
        float w = (float)media.width;
        float ratio = Math.Min(h, w) / Math.Max(h, w);

        Vector3 scale = new Vector3(1f,1f,1f);
        
        
        paintingParent.localScale = new Vector3(1f, 1f, 1f);
        //If TALLER than WIDE, make X small (skinny)
        if (h > w){
            scale = new Vector3(ratio, 1f, 1f);
            //paintingParent.localScale = new Vector3(ratio, 1f, 1f);
        }

        //Else if WIDER than TALL, make Z small (portrait orientation)
        else if (w > h){
            scale = new Vector3(1f, 1f, ratio);
            //paintingParent.localScale = new Vector3(1f, ratio, 1f);
        }
        Vector3 local = paintingParent.transform.localScale;
        scale = new Vector3(scale.x * local.x, scale.y * local.y, local.z);
        paintingParent.transform.localScale = scale;
    }
    IEnumerator _initMedia()
    {
        if (Application.isEditor == false || media != null)
            yield break;
        yield return null;
        yield return null;
        if (media == null)
        {
            media = ScriptableObject.CreateInstance<MediaSO>();
            
            EditorUtility.SetDirty(gameObject);
            
        }
        

    }

    // Update is called once per frame
    void Update()
    {
    }
}
