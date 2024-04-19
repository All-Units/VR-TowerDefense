using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class TutorialBubble : MonoBehaviour
{
    public float TimeToFill = 0.8f;
    public Image fillCircle;


    public UnityEvent OnCircleFill;

    XRSimpleInteractable simple;
    // Start is called before the first frame update
    void Start()
    {
        if (fillCircle != null)
            fillCircle.fillAmount = 0;
        simple = GetComponent<XRSimpleInteractable>();
        simple.activated.AddListener(_StartFill);
        simple.deactivated.AddListener(_EndFill);

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void _StartFill(ActivateEventArgs a)
    {
        _currentFillRoutine = _SkipRound();
        StartCoroutine(_currentFillRoutine);
    }
    void _EndFill(DeactivateEventArgs a)
    {
        _StopFill();
    }

    void _StopFill()
    {
        if (_currentFillRoutine != null)
            StopCoroutine(_currentFillRoutine);
        fillCircle.fillAmount = 0;
    }
    IEnumerator _currentFillRoutine = null;
    IEnumerator _SkipRound()
    {
        
        float t = 0f;
        Image fill = fillCircle;
        fill.fillAmount = 0f;
        while (t <= TimeToFill)
        {
            
            yield return null;
            t += Time.deltaTime;
            fill.fillAmount = math.lerp(0f, 1f, (t / TimeToFill));
        }
        
        if (t >= TimeToFill)
        {
            yield return new WaitForSeconds(0.1f);
            OnCircleFill.Invoke();
        }

        _currentFillRoutine = null;
    }
}
