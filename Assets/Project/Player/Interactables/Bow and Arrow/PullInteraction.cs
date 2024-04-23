using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class PullInteraction : XRBaseInteractable
{
    public static event Action PullActionStarted;
    public static event Action<float, TowerPlayerWeapon> PullActionReleased;
    
    public Transform start, end;
    public GameObject notch;
    public float pullAmount { get; private set; } = 0.0f;
    private float pullIncrement = 0.1f;

    private LineRenderer _lineRenderer;
    private IXRSelectInteractor pullingInteractor = null;

    public AnimationCurve curve;
    [SerializeField] private AudioClipController drawAudio;
    private TowerPlayerWeapon _playerWeapon;

    protected override void Awake()
    {
        base.Awake();
        _lineRenderer = GetComponent<LineRenderer>();
        XRPauseMenu.OnPause += _DestroyArrow;
    }

    private void Start()
    {
        _playerWeapon = GetComponentInParent<TowerPlayerWeapon>();
    }

    void _DestroyArrow()
    {
        if (notch.transform.childCount != 0)
            Destroy(notch.transform.GetChild(0).gameObject);
    }

    public void SetPullInteractor(SelectEnterEventArgs args)
    {
        pullingInteractor = args.interactorObject;
        PullActionStarted?.Invoke();
    }

    public void Release()
    {
        PullActionReleased?.Invoke(pullAmount, _playerWeapon);
        pullingInteractor = null;
        pullAmount = 0f;
        pullIncrement = 0.1f;
        notch.transform.localPosition =
            new Vector3(notch.transform.localPosition.x, notch.transform.localPosition.y, 0);
        UpdateString();
    }


    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        base.ProcessInteractable(updatePhase);
        
        if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
        {
            if (isSelected)
            {
                var pullPosition = pullingInteractor.transform.position;
                var prevPull = pullAmount;
                
                pullAmount = CalculatePull(pullPosition);
                //Started being pulled this frame
                if (prevPull == 0f && pullAmount != 0f)
                {
                    drawAudio.PlayClip();
                }
                
                UpdateString();
                if(pullAmount >= pullIncrement || pullAmount <= pullIncrement - 0.1f)
                    HapticFeedback();
            }
        }
    }

    private float CalculatePull(Vector3 pullPosition)
    {
        var pullDirection = pullPosition - start.position;
        var targetDirection = end.position - start.position;
        var maxLength = targetDirection.magnitude;
        
        targetDirection.Normalize();
        var pullValue = Vector3.Dot(pullDirection, targetDirection) / maxLength;
        return Mathf.Clamp01(pullValue);
    }

    private void UpdateString()
    {
        var linePosition = Vector3.forward * Mathf.Lerp(start.transform.localPosition.z, end.transform.localPosition.z, pullAmount);
        notch.transform.localPosition = new Vector3(notch.transform.localPosition.x, notch.transform.localPosition.y, linePosition.z + 0.2f);
        _lineRenderer.SetPosition(1, linePosition + new Vector3(0,0, 0.2f));
    }

    private void HapticFeedback()
    {
        if (pullingInteractor != null)
        {
            var currentController = pullingInteractor.transform.gameObject.GetComponentInParent<ActionBasedController>();
            currentController.SendHapticImpulse(curve.Evaluate(pullAmount), 0.1f);
            if (pullAmount >= pullIncrement)
            {
                pullIncrement += .1f;
            }
            else
            {
                pullIncrement -= .1f;
            }
        }
    }
}