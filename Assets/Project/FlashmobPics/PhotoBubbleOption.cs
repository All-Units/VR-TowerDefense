using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.Video;
using UnityEngine.XR.Interaction.Toolkit;

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
    [HideInInspector] public PhotoBubbleGroup parentGroup;
    [HideInInspector] public bool IS_CENTER = false;
    [SerializeField] public MediaSO media;
    
    [SerializeField] Transform bubbleParent;
    [SerializeField] MeshRenderer photoMesh;
    [SerializeField] Transform displayParent;

    [SerializeField] ParticleSystem selectedParticles;
    [SerializeField] GameObject soundIcon;
    Material mat;
    VideoPlayer videoPlayer
    {
        get
        {
            if (_player == null)
            {
                _player = photoMesh.GetComponent<VideoPlayer>();
                if (_player == null)
                    _player = photoMesh.gameObject.AddComponent<VideoPlayer>();
            }
            return _player;
        }
    }
    VideoPlayer _player;
    XRSimpleInteractable interactable
    {
        get
        {
            if (_interactable == null)
                _interactable = GetComponentInChildren<XRSimpleInteractable>();
            return _interactable;
        }
    }
    XRSimpleInteractable _interactable;

    [SerializeField] Material blankVideoTexture;
    [SerializeField] Material basePhotoTexture;
    // Start is called before the first frame update
    void Start()
    {
        Resize();

        interactable.hoverEntered.AddListener(_onHoverStart);
        interactable.hoverExited.AddListener(_onHoverEnd);

        interactable.activated.AddListener(_OnClick);


        if (selectedParticles != null)
            selectedParticles = GetComponentInChildren<ParticleSystem>();
        selectedParticles.Stop();
    }
    void _onHoverStart(HoverEnterEventArgs args)
    {
        if (IS_CENTER) 
            return;
        selectedParticles.Play();
    }
    void _onHoverEnd(HoverExitEventArgs args)
    {
        if (IS_CENTER) 
            return;
        selectedParticles.Stop();
    }
    void _OnClick(ActivateEventArgs args)
    {
        if (IS_CENTER) 
            return;
        if (parentGroup == null)
            return;
        if (media.video)
        {
            videoPlayer.Stop();
            videoPlayer.Play();
        }
        parentGroup.SetCenterPicture(media);
    }
    
    public void Resize()
    {
        if (media.video != null)
        {
            photoMesh.material = blankVideoTexture;
            if (soundIcon)
            {
                soundIcon.SetActive(media.usesSound);
            }
        }
        else if (media.picture != null)
        {
            photoMesh.material = basePhotoTexture;
        }
        if (Application.isEditor && false)
        {
            mat = photoMesh.sharedMaterial;
        }
        else
        {
            mat = photoMesh.material;
        }
        


        if (media.picture)
        {
            mat.mainTexture = media.picture;
            videoPlayer.enabled = false;
            videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
            soundIcon.SetActive(false);
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
                //Debug.Log($"Player? {videoPlayer != null}. Media? {media != null}. Clip? {media.video != null}");
                videoPlayer.clip = media.video;
                videoPlayer.playOnAwake = true;
                videoPlayer.isLooping = true;
                // Direct if uses sound AND IS THE CENTER, otherwise nothing
                videoPlayer.audioOutputMode = (media.usesSound && IS_CENTER) ?
                    VideoAudioOutputMode.AudioSource : VideoAudioOutputMode.None;
                if (IS_CENTER)
                    Debug.Log($"Setting audio to: {videoPlayer.audioOutputMode}");
                videoPlayer.SetTargetAudioSource(0, videoPlayer.GetComponent<AudioSource>());
                videoPlayer.Play();
            }
            StartCoroutine(_setNull());
            
            
        }

        float h = (float)media.height;
        float w = (float)media.width;
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
        this.media = media;
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
