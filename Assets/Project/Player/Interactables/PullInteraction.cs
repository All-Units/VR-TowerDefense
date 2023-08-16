using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class PullInteraction : XRBaseInteractable
{
    public static event Action<float> PullActionReleased;

    public Transform start, end;
    public GameObject notch;
    public float pullAmount { get; private set; } = 0.0f;

    private LineRenderer _lineRenderer;
    private IXRSelectInteractor pullingInteractor = null;

    protected override void Awake()
    {
        base.Awake();
        _lineRenderer = GetComponent<LineRenderer>();
    }

    public void SetPullInteractor(SelectEnterEventArgs args)
    {
        pullingInteractor = args.interactorObject;
    }

    public void Release()
    {
        PullActionReleased?.Invoke(pullAmount);
        pullingInteractor = null;
        pullAmount = 0f;
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
                pullAmount = CalculatePull(pullPosition);
                
                UpdateString();
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
        _lineRenderer.SetPosition(1, linePosition);
    }
}