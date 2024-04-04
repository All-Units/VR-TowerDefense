using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;

public class SplashScreenManager : MonoBehaviour
{
    public TeleportationProvider teleporter;
    public GameLevel_SO MainMenu;
    public float DisplayTime = 4f;
    [SerializeField]
    public float CenteredCamAngle = 0f;
    public float CamY;
    float _startCamAngle;
    float cam_y => _camTransform.eulerAngles.y;
    Transform _camTransform;
    private void Awake()
    {
        _camTransform = Camera.main.transform;
        StartCoroutine(_CenterCamera());
    }
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(_LoadMainMenu());
    }
    IEnumerator _CenterCamera()
    {
        float startTime = Time.time;
        _startCamAngle = CenteredCamAngle;
        float t = 0f;
        float deltaY;
        while (t < 10f)
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

    // Update is called once per frame
    void Update()
    {
        if (gameObject.activeInHierarchy == false) return;
        CamY = cam_y;
    }
    IEnumerator _LoadMainMenu()
    {
        PlayableDirector pd = GetComponentInChildren<PlayableDirector>();
        if (pd != null)
        {
            DisplayTime = (float)pd.duration;
        }
        yield return new WaitForSeconds(DisplayTime);
        var loader = SceneManager.LoadSceneAsync(MainMenu.levelTitle);
        float t = Time.time;
        while (loader.isDone == false)
        {
            yield return null;
        }
    }
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (EditorApplication.isPlaying) return;
        if (_camTransform == null) _camTransform = Camera.main.transform;
        CenteredCamAngle = cam_y;
    }
#endif
    void RecenterCamera()
    {
        Vector3 euler = Vector3.zero;
        euler.y = CenteredCamAngle;
        Vector3 pos = _camTransform.position;
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
