using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;
using Slider = UnityEngine.UI.Slider;

public class HealthbarController : MonoBehaviour
{
    [SerializeField] private Slider slider;

    [SerializeField] private HealthController healthController;

    private bool _isShowing = false;

    private void Start()
    {
        if (healthController == null)
        {
            healthController = GetComponentInParent<HealthController>();
            if (healthController == null)
            {
                Debug.LogError($"Unable to find health component on {transform.parent.gameObject.name}!", gameObject);
                return;
            }
        }

        var tower = GetComponentInParent<Tower>();
        if (tower)
        {
            tower.OnSelected += () => gameObject.SetActive(false);
            tower.OnDeselected += () => gameObject.SetActive(true);
        }

        healthController.OnTakeDamage += UpdateValue;
        slider.maxValue = healthController.MaxHealth;
        slider.value = healthController.CurrentHealth;
        _isShowing = true;
        //UpdateValue(healthController.CurrentHealth);
        HideInstantly();    
    }

    private void UpdateValue(int curr)
    {
        slider.value = curr;
        
        if (healthController.CurrentHealth < healthController.MaxHealth)
        {
            Show();
        }   
        else
        {
            Hide();
        }
    }

    private void Show()
    {
        if(_isShowing) return;
        
        StartCoroutine(Fade(.5f, true));
        _isShowing = true;
    }

    private void Hide()
    {
        if(!_isShowing) return;

        StartCoroutine(Fade(.5f, false));
        _isShowing = false;
    }
    void HideInstantly()
    {
        if (!_isShowing) return;
        StartCoroutine(Fade(0f, false));
        _isShowing = false;
    }

    private IEnumerator Fade(float time, bool fadeIn)
    {
        var images = GetComponentsInChildren<Image>();
        var t = time;

        while (t > 0)
        {
            foreach (var image in images)
            {
                var imageColor = image.color;
                imageColor.a = fadeIn ? Mathf.Lerp(0, 1, t / time) : Mathf.Lerp(1, 0, t / time);
                image.color = imageColor;
            }
            
            yield return null;
            t -= Time.deltaTime;
        }
        
        foreach (var image in images)
        {
            var imageColor = image.color;
            imageColor.a = fadeIn ? 1 : 0;
            image.color = imageColor;
        }
    }
    
}
