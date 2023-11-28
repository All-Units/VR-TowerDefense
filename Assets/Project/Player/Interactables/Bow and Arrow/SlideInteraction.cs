using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public class SlideInteraction : XRBaseInteractable
{
    public event Action PullActionStarted;
    public event Action<float> PullActionReleased;

    public UnityEvent<float> updatePullAmount;
    public UnityEvent onLocked;

    public Transform start, end;
    public float pullAmount { get; private set; } = 0.0f;
    private float pullIncrement = 0.1f;
    public float maxPullIncrement = 0.1f;

    private IXRSelectInteractor pullingInteractor = null;

    public AnimationCurve curve;

    [SerializeField] private bool lockAtEnd = true;
    public bool isLocked { get; private set; } = false;

    public void SetPullInteractor(SelectEnterEventArgs args)
    {
        pullingInteractor = args.interactorObject;
        PullActionStarted?.Invoke();
    }

    public void Release()
    {
        PullActionReleased?.Invoke(pullAmount);
        pullingInteractor = null;
        
        if(isLocked) return;
        
        pullAmount = 0f;
        pullIncrement = 0.1f;

        updatePullAmount?.Invoke(pullAmount);
    }


    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        base.ProcessInteractable(updatePhase);

        if(isLocked) return;
        
        if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
        {
            if (isSelected)
            {
                pullAmount = CalculatePull(pullingInteractor.transform.position);

                if (!isLocked)
                {
                    if (pullAmount >= .998f)
                    {
                        Lock();
                    }
                }

                updatePullAmount?.Invoke(pullAmount);
                
                if (pullAmount >= pullIncrement || pullAmount <= pullIncrement - 0.1f)
                    HapticFeedback();
            }
        }
    }

    private void Lock()
    {
        isLocked = true;
        onLocked?.Invoke();
        interactionManager.SelectExit(pullingInteractor, this);
    }

    public void Unlock()
    {
        isLocked = false;
        Release();
    }

    private float CalculatePull(Vector3 pullPosition)
    {
        var pullDirection = pullPosition - start.position;
        var targetDirection = end.position - start.position;
        var maxLength = targetDirection.magnitude;
        
        targetDirection.Normalize();
        var pullValue = Vector3.Dot(pullDirection, targetDirection) / maxLength;

        pullValue = Mathf.Min(pullValue, pullAmount + (maxPullIncrement * Time.deltaTime));
        return Mathf.Clamp01(pullValue);
    }



    private void HapticFeedback()
    {
        if (pullingInteractor != null)
        {
            var currentController =
                pullingInteractor.transform.gameObject.GetComponentInParent<ActionBasedController>();
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