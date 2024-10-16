using System.Collections;
using UnityEngine;
using UnityEngine.XR.Content.Interaction;
using Image = UnityEngine.UI.Image;
using Slider = UnityEngine.UI.Slider;

public class HealthbarController : MonoBehaviour
{
    [SerializeField] private Slider slider;

    [SerializeField] private HealthController healthController;
    /*[Tooltip("Unless more damage is taken, hides after this many seconds")]
    public int HideAfter = 1;*/

    private bool _isShowing = false;
    public bool AlwaysShowing;

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
        healthController.onDeath.AddListener(_Destroy);

        var tower = GetComponentInParent<Tower>();
        if (tower)
        {
            tower.OnSelected += () => slider.gameObject.SetActive(false);
            tower.OnDeselected += () => slider.gameObject.SetActive(true);
            
            if (tower is ProjectileTower projectileTower)
            {
                projectileTower.onTakeover.AddListener(() => slider.gameObject.SetActive(false));
                projectileTower.onRelease.AddListener(() => slider.gameObject.SetActive(true));
            }
        }

        healthController.OnTakeDamage += UpdateValue;
        slider.maxValue = healthController.MaxHealth;
        slider.value = healthController.CurrentHealth;
        _isShowing = true;
        //UpdateValue(healthController.CurrentHealth);
        HideInstantly();
        if (AlwaysShowing)
        {
            Show();
        }

        
    }
    

    private void UpdateValue(int curr)
    {
        slider.maxValue = 1f;
        slider.minValue = 0f;
        slider.value = ((float)healthController.CurrentHealth / (float)healthController.MaxHealth);
        
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
        if (gameObject.activeInHierarchy == false) return;
        StartCoroutine(Fade(.5f, true));
        /*if (_currentFader != null)
            StopCoroutine(_currentFader);
        _currentFader = _FadeAfterDelay();
        StartCoroutine(_currentFader);*/
        _isShowing = true;
    }
    /*IEnumerator _currentFader = null;
    IEnumerator _FadeAfterDelay()
    {

        yield return new WaitForSeconds(HideAfter);
        _currentFader = null;
        Hide();

    }*/

    private void Hide()
    {
        if(!_isShowing) return;
        if(AlwaysShowing) return;

        StartCoroutine(Fade(.5f, false));
        _isShowing = false;
    }
    void HideInstantly()
    {
        if (!_isShowing) return;
        StartCoroutine(Fade(0f, false));
        _isShowing = false;
    }
    void _Destroy() { Destroy(gameObject); }

    private IEnumerator Fade(float time, bool fadeIn)
    {
        var images = GetComponentsInChildren<Image>();
        var t = time;

        while (t > 0)
        {
            foreach (var image in images)
            {
                var imageColor = image.color;
                if (healthController.isDead) { fadeIn = false; t = 0f; }
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
