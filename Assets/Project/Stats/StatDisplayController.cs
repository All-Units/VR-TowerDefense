using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class StatDisplayController : MonoBehaviour
{
    [FormerlySerializedAs("statTracker")] [SerializeField] private StatDisplayModel statDisplayModel;

    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text valueText;

    [SerializeField] private Animator animator;

    private void OnEnable()
    {
        if(statDisplayModel.statTrackers.All(stat => stat.getSerializeValue == 0))
            Destroy(transform.parent.gameObject);
        titleText.text = statDisplayModel.displayName;

        foreach (var tracker in statDisplayModel.statTrackers)
        {
            var text = Instantiate(valueText, titleText.transform.parent);
            text.text = $"{tracker.statName}: {tracker.getSerializeValue}";
        }
        if (animator != null)
            OnDropItem();
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