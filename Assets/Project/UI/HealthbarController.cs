using System.Collections;
using UnityEngine;
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
    PlayerControllableTower controllableTower;
    bool IsPlayerControlled => controllableTower != null && controllableTower.isPlayerControlled;

    private void Start()
    {
        if ( controllableTower == null)
        {
            controllableTower = GetComponentInParent<PlayerControllableTower>();
            if (controllableTower == null)
                controllableTower = GetComponentInChildren<PlayerControllableTower>();
        }
        
        if (healthController == null)
        {
            healthController = GetComponentInParent<HealthController>();
            if (healthController == null)
            {
                Debug.LogError($"Unable to find health component on {transform.parent.gameObject.name}!", gameObject);
                return;
            }
        }
        
        InitializeHealthController();

        var tower = GetComponentInParent<Tower>();
        if (tower)
        {
            tower.OnSelected += _Disable;
            tower.OnDeselected += _Enable;
            
            if (tower is ProjectileTower projectileTower)
            {
                projectileTower.onTakeover.AddListener(_Disable);
                projectileTower.onRelease.AddListener(_Enable);
            }
        }


        _isShowing = true;
        //UpdateValue(healthController.CurrentHealth);
        HideInstantly();
        if (AlwaysShowing)
        {
            Show();
        }
    }

    void _Disable()
    {
        slider.gameObject.SetActive(false);
    }
    void _Enable()
    {

        if (IsPlayerControlled) { return; }
        slider.gameObject.SetActive(true);
        if (healthController.CurrentHealth < healthController.MaxHealth)
        {
            _ShowInstantly();
        }
    }
    private void OnDestroy()
    {
        var tower = GetComponentInParent<Tower>();
        if (tower)
        {
            tower.OnSelected -= _Disable;
            tower.OnDeselected -= _Enable;

            if (tower is ProjectileTower projectileTower)
            {
                projectileTower.onTakeover.RemoveListener(_Disable);
                projectileTower.onRelease.RemoveListener(_Enable);
            }

        }
        healthController.OnTakeDamage -= UpdateValue;
    }

    public void SetHealthController(HealthController controller)
    {
        if (healthController != null)
        {
            healthController.OnTakeDamage -= UpdateValue;
            healthController.onDeath.RemoveListener(_Destroy);
        }

        healthController = controller;
        InitializeHealthController();
    }
    
    private void InitializeHealthController()
    {
        healthController.onDeath.AddListener(_Destroy);
        healthController.OnTakeDamage += UpdateValue;
        slider.maxValue = healthController.MaxHealth;
        slider.value = healthController.CurrentHealth;
    }

    public void UpdateValue(int curr)
    {
        slider.maxValue = 1f;
        slider.minValue = 0f;
        slider.value = ((float)healthController.CurrentHealth / (float)healthController.MaxHealth);
        if (controllableTower != null && controllableTower.isPlayerControlled)
        {
            HideInstantly();
            return;
        }
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
        
        _isShowing = true;
    }
    

    private void Hide()
    {
        if(!_isShowing) return;
        if(AlwaysShowing) return;

        StartCoroutine(Fade(.5f, false));
        _isShowing = false;
    }
    void HideInstantly()
    {
        //if (!_isShowing) return;
        StartCoroutine(Fade(0f, false));
        _isShowing = false;
    }
    void _ShowInstantly()
    {
        StartCoroutine(Fade(0f, true));
        _isShowing = true;
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
            if (time == 0f)
                break;
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