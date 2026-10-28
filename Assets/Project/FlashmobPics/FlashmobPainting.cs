using System.Collections;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Video;

[CustomEditor(typeof(FlashmobPainting))]
public class PaintingEditor : Editor
{
    FlashmobPainting p => (FlashmobPainting)target;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        return;
        if (p && p.HasPainting)
        {
            //GUILayout.Label(p.previewIcon);
        }
        else if (p && p.HasPainting == false)
        {
            GUILayout.Label("\n\n\t\tNO MEDIA\n\n");
        }
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
    VideoPlayer videoPlayer
    {
        get
        {
            return null;
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
        if (Application.isEditor == false)
        {
            //Resize();
        }
    }
    public void Resize()
    {
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
            paintingParent.localScale = new Vector3(ratio, 1f, 1f);
        }

        //Else if WIDER than TALL, make Z small (portrait orientation)
        else if (w > h){
            scale = new Vector3(1f, 1f, ratio);
            paintingParent.localScale = new Vector3(1f, ratio, 1f);
        }

        paintingRenderer.transform.localScale = scale;
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
