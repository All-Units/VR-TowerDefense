using UnityEngine;
using UnityEngine.UI;

public class SliderController : MonoBehaviour
{
    [SerializeField] private Slider slider;

    public void SetBounds(int max, int min = 0)
    {
        slider.maxValue = max;
        slider.minValue = min;
    }

    public void UpdateValue(int v)
    {
        slider.value = v;
    }
}