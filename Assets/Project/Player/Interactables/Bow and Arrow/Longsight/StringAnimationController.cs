using UnityEngine;

public class StringAnimationController : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private Transform start, end;

    public void UpdateStringPull(float pullAmount)
    {
        var linePosition = Vector3.forward * Mathf.Lerp(start.localPosition.z, end.localPosition.z, pullAmount);
        lineRenderer.SetPosition(1, linePosition + new Vector3(0,0, 0.2f));
    }
}
