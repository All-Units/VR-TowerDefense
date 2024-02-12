using TMPro;
using UnityEngine;

public class StatDisplayController : MonoBehaviour
{
    [SerializeField] private StatTracker statTracker;

    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text valueText;

    [SerializeField] private Animator animator;

    private void Start()
    {
        titleText.text = statTracker.displayName;
        valueText.text = $"{statTracker.statName}: {statTracker.getSerializeValue}";
    }

    public void OnPickupItem()
    {
        animator.Play("OpenDisplay");
    }

    public void OnDropItem()
    {
        animator.Play("CloseDisplay");
    }
}
