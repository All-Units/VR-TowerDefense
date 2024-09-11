using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;


[RequireComponent(typeof(XRSimpleInteractable))]
public class GrabAndReposition : MonoBehaviour
{
    public float DeadZone = 0.3f;
    XRSimpleInteractable simple;
    [SerializeField] float rotateSensitivity = 1f;
    [SerializeField] float distanceToCamSensitivity = 1f;
    [SerializeField] float distanceToCamStickSensitivity = 15f;
    [SerializeField] float verticalSensitivity = 1f;
    [SerializeField] float verticalStickSensitivity = 15f;
    [SerializeField] Vector2 distanceToCamBounds = new Vector2(2f, 5f);
    [SerializeField] Vector2 heightBounds = new Vector2(0.6f, 3f);
    [SerializeField] bool invertY = true;
    [SerializeField] Transform canvasParent;
    [SerializeField] Transform canvasSecondLayer;
    [SerializeField] Color grabbedColor = Color.white;
    [SerializeField] Color releasedColor = Color.white;
    void Awake()
    {
        _InitComponents();
        _InitSimpleXREvents();
        
        _SetColor(releasedColor);
        localRotStart = transform.localRotation;
        localStart = transform.localPosition;

        debugText01.text = "";
    }

    MeshRenderer mr;
    /// <summary>
    /// Initialize all monobehavior references
    /// </summary>
    void _InitComponents()
    {
        simple = GetComponent<XRSimpleInteractable>();
        mr = GetComponent<MeshRenderer>();
    }
    /// <summary>
    /// Adds listeners to all simple xr events
    /// </summary>
    void _InitSimpleXREvents()
    {
        simple.firstSelectEntered.AddListener(_Grabbed);
        simple.lastSelectExited.AddListener(_Released);
        simple.selectEntered.AddListener(_OnAnyGrab);
    }
    Vector3 localStart;
    Quaternion localRotStart;
    
    void _SetColor(Color c)
    {
        mr.material.color = c;
    }
    bool _isGrabbed = false;
    public InputActionReference leftMove;
    public InputActionReference rightMove;
    InputAction _lastMove = null;
    public TextMeshProUGUI debugText01;
    IXRSelectInteractor _currentTor = null;
    void _Grabbed(SelectEnterEventArgs a)
    {
        _currentTor = a.interactorObject;
        _SetColor(grabbedColor);
        Transform hand = a.interactorObject.transform;
        Inventory2 i2 = GetInvByTransform(hand);
        InputActionReference moveRef = leftMove;
        if (i2 == null)
        {
            string error = $"No inv on {hand.name}";
            Debug.LogError(error, hand);
            debugText01.text = error;
        }
            
        if (i2.whichHand == WhichHand.right)
            moveRef = rightMove;
        _lastMove = moveRef.action;

        _lastMove.performed += _MoveJoystick;
        _lastMove.canceled += _JoystickReleased;
        if (_currentFollower != null)
            StopCoroutine(_currentFollower);
        _currentFollower = _FollowHandRoutine(hand);
        StartCoroutine(_currentFollower);
    }
    Dictionary<Transform, Inventory2> invByTransform = new Dictionary<Transform, Inventory2>();
    Inventory2 GetInvByTransform(Transform hand)
    {
        Inventory2 i2 = null;
        //Cache inventories by transform
        if (invByTransform.ContainsKey(hand) == false)
        {
            i2 = hand.GetComponentInParent<Inventory2>();
            if (i2 == null)
            {
                Debug.LogError($"No Inventory on: {hand.name}", hand);
            }
            invByTransform.Add(hand, i2);
        }
        else
            i2 = invByTransform[hand];

        return i2;
    }
    void _OnAnyGrab(SelectEnterEventArgs a)
    {
        if (_currentTor != null && _currentTor != a.interactorObject)
        {
            simple.interactionManager.SelectExit(_currentTor, _currentTor.firstInteractableSelected);
        }
    }
    void _Released(SelectExitEventArgs a) 
    { 
        _lastMove.performed -= _MoveJoystick;
        _lastMove.canceled -= _JoystickReleased;
        _lastMove = null;
        _isGrabbed = false;
        _SetColor(releasedColor);
        transform.localPosition = localStart;
        transform.localRotation = localRotStart;
        Quaternion rot = transform.rotation;
        IEnumerator _Freeze()
        {
            yield return null;
            yield return null;
            transform.rotation = rot;
        }
        StartCoroutine(_Freeze());
        _currentTor = null;

    }
    bool _joystickHeld = false;
    void _JoystickReleased(InputAction.CallbackContext callback)
    {
        _joystickHeld = false;
    }
    void _MoveJoystick(InputAction.CallbackContext callback)
    {
        _joystickHeld = true;
        Vector2 dir = callback.ReadValue<Vector2>();
        if (_currentJoystickRoutine != null)
            StopCoroutine(_currentJoystickRoutine);
        _currentJoystickRoutine = _HandleJoysticksRoutine(dir);
        StartCoroutine(_currentJoystickRoutine);
    }
    IEnumerator _currentJoystickRoutine = null;
    IEnumerator _HandleJoysticksRoutine(Vector2 dir)
    {
        while (_joystickHeld)
        {
            _HandleJoystickHorizontal(dir.x);
            _HandleJoystickVertical(dir.y);
            yield return null;
        }
        _currentJoystickRoutine = null;
        
    }
    void _HandleJoystickHorizontal(float x)
    {
        if (Mathf.Abs(x) <= DeadZone) return;
        float delta = x * Time.deltaTime * distanceToCamStickSensitivity;
        _ChangeDistanceToCam(delta);
        
    }
    void _HandleJoystickVertical(float y)
    {
        if (Mathf.Abs(y) <= DeadZone) return;
        float delta = y * Time.deltaTime * verticalStickSensitivity;
        _ChangeHeight(delta);
        //canvasParent.position += new Vector3(0f, delta, 0f);
    }
    Transform cam => InventoryManager.instance.playerCameraTransform;
    IEnumerator _currentFollower = null;
    IEnumerator _FollowHandRoutine(Transform hand)
    {
        if (canvasParent == null) yield break;
        _isGrabbed = true;
        Vector3 lastHandPos = hand.transform.position;
        Vector3 cameraPos = cam.position;
        Vector3 cameraDelta = cameraPos - lastHandPos;
        cameraDelta.y = 0f;
        Vector3 cameraRot = Quaternion.LookRotation(cameraDelta).eulerAngles;
        Vector3 lastCameraRot = cameraRot;
        Vector3 lastCameraPos = cameraPos;
        while (_isGrabbed)
        {
            yield return null;
            Vector3 handPos = hand.transform.position;
            cameraDelta = cameraPos - handPos; cameraDelta.y = 0f;
            cameraRot = Quaternion.LookRotation(cameraDelta).eulerAngles;
            float yDelta = cameraRot.y - lastCameraRot.y;
            yDelta *= rotateSensitivity * Time.deltaTime;
            
            //Don't change to MASSIVE deltas
            if (Mathf.Abs(yDelta) <= 5f)
                canvasParent.eulerAngles += new Vector3(0f, yDelta, 0f);

            float verticalDelta = handPos.y - lastHandPos.y;
            verticalDelta *= (verticalSensitivity * Time.deltaTime);
            _ChangeHeight(verticalDelta);
            //canvasParent.position += new Vector3(0f, verticalDelta, 0f);
            
            //Current distance minus last distance
            float distanceDelta = Utilities.FlatDistance(handPos, cameraPos) - 
                Utilities.FlatDistance(lastHandPos, lastCameraPos);
            distanceDelta *= (Time.deltaTime * distanceToCamSensitivity);
            _ChangeDistanceToCam(distanceDelta);
            //canvasSecondLayer.localPosition += new Vector3(0f, 0f, distanceDelta);


            lastHandPos = handPos;
            

            lastCameraRot = cameraRot;
            lastCameraPos = cameraPos;
        }
    }
    void _ChangeDistanceToCam(float delta)
    {
        Vector3 pos = canvasSecondLayer.localPosition;
        pos.z += delta;
        float z = pos.z;
        //Only apply the change if it is within bounds
        if (z >= distanceToCamBounds.x && z <= distanceToCamBounds.y)
        {
            canvasSecondLayer.localPosition = pos;
        }
    }
    void _ChangeHeight(float delta)
    {
        Vector3 pos = canvasSecondLayer.localPosition;
        pos.y += delta;
        
        float y = pos.y;
        //Only move if new y is within the bounds
        if (y >= heightBounds.x && y <= heightBounds.y)
        {
            canvasSecondLayer.localPosition = pos;

        }
        
    }



}
