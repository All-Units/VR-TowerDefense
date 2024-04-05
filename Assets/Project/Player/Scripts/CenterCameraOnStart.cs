using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[ExecuteInEditMode]
public class CenterCameraOnStart : MonoBehaviour
{
    [Tooltip("Global euler Y of the player camera")]
    [SerializeField] float CenteredCamAngle;

    [SerializeField][HideInInspector] Camera cam { get 
        { 
            if (_cam == null)
                _cam = GetComponentInChildren<Camera>();
        return _cam;
        } 
    }
    [SerializeField]
    [HideInInspector] Camera _cam;
    TeleportationProvider teleporter { get
        {
            if (_teleporter == null)
                _teleporter = GetComponentInChildren<TeleportationProvider>();
            return _teleporter;
        } }
    [SerializeField]
    [HideInInspector] TeleportationProvider _teleporter;
    
    private Transform camTransform => cam.transform;
    float cam_y => camTransform.eulerAngles.y;
    

    private void Awake()
    {
        StartCoroutine(_CenterCamera());
    }
    /// <summary>
    /// Checks for the first few seconds of gameplay if the player's head is off center, and centers if needed
    /// </summary>
    /// <returns></returns>
    IEnumerator _CenterCamera()
    {
        float _startCamAngle = CenteredCamAngle;
        float t = 0f;
        float deltaY;
        while (t < 1.2f)
        {
            t += Time.deltaTime;
            yield return null;
            deltaY = math.abs(cam_y - _startCamAngle);
            if (deltaY > 5f)
            {
                RecenterCamera();
                yield break;
            }
        }
    }
    #if UNITY_EDITOR
    [ExecuteInEditMode]
    // Update is called once per frame
    void Update()
    {
        _UpdateCamAngle();
    }

    void _UpdateCamAngle()
    {

        if (EditorApplication.isPlaying) return;
        CenteredCamAngle = cam_y;

        
    }
    #endif
    /// <summary>
    /// Centers the camera rotation based on the cached forward direction
    /// </summary>
    void RecenterCamera()
    {
        Vector3 euler = Vector3.zero;
        euler.y = CenteredCamAngle;
        Vector3 pos = teleporter.transform.position;
        TeleportRequest request = new TeleportRequest()
        {
            requestTime = Time.time,
            matchOrientation = MatchOrientation.TargetUpAndForward,

            destinationPosition = pos,
            destinationRotation = Quaternion.Euler(euler)
        };
        teleporter.QueueTeleportRequest(request);
    }
}
