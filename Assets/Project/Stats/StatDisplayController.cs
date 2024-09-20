using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit;

public class StatDisplayController : MonoBehaviour
{
    [FormerlySerializedAs("statTracker")] [SerializeField] private StatDisplayModel statDisplayModel;

    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text valueText;

    [SerializeField] private Animator animator;

    private void OnEnable()
    {
        foreach (var stat in statDisplayModel.statTrackers)
            stat.SerializeIfChanged();

        if(statDisplayModel.statTrackers.All(stat => stat.getSerializeValue == 0))
            Destroy(transform.parent.gameObject);
        titleText.text = statDisplayModel.displayName;

        foreach (var tracker in statDisplayModel.statTrackers)
        {
            var text = Instantiate(valueText, titleText.transform.parent);
            text.text = $"{tracker.statName}: {tracker.getSerializeValue}";
        }

        XRGrabInteractable grab = GetComponentInParent<XRGrabInteractable>();
        grab.firstSelectEntered.AddListener(OnPickupItem);
        grab.lastSelectExited.AddListener(OnDropItem);

        grab.firstHoverEntered.AddListener(_OnHoverStart);
        grab.lastHoverExited.AddListener(_OnHoverEnd);

        if (animator != null)
            _CloseDisplay();
    }
    void _OnHoverStart(HoverEnterEventArgs a)
    {
        if (_held) return;
        _OpenDisplay();
    }
    void _OnHoverEnd(HoverExitEventArgs a)
    {
        if (_held) return;
        _CloseDisplay();

    }
    bool _held = false;
    public void OnPickupItem(SelectEnterEventArgs a)
    {
        _OpenDisplay();
        _held = true;
    }

    public void OnDropItem(SelectExitEventArgs a)
    {
        _held = false;
        _CloseDisplay();
    }
    void _OpenDisplay() { animator.Play("OpenDisplay"); }
    void _CloseDisplay() { animator.Play("CloseDisplay");}
}