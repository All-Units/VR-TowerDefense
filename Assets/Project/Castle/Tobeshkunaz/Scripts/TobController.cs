using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(Animator))]
public class TobController : MonoBehaviour
{
    private Animator _anim;
    [Header("Heart vars")]
    [SerializeField] GameObject hearticles;
    [SerializeField] Transform _heartPos;
    [SerializeField] float _heartTime = 1.5f;
    [SerializeField] float _timeBetweenHearts = .4f;
    [SerializeField] float _heartRadius = 1f;
    [Header("Food Variables")]
    [SerializeField] Transform foodPoint;
    [SerializeField] float foodMovementTime = 1f;
    [SerializeField] float timeBeforeDestroyFood = 0.8f;

    [SerializeField] float eatingCooldown = 10f;
    void _CheckForAnim() { if (_anim == null) _anim = GetComponent<Animator>(); }
   
    // Start is called before the first frame update
    void Start()
    {
        _CheckForAnim();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag == "DragonFood")
        {
            StartCoroutine(_FoodMovementRoutine(collision.gameObject));
            
        }
        
    }
    bool _canEat = true;
    IEnumerator _FoodMovementRoutine(GameObject food)
    {
        //Do nothing if this isn't food
        if (food.gameObject.tag != "DragonFood") yield break;

        //Do nothing if we ate too recently
        if (_canEat == false) yield break;
        _canEat = false;
        
        XRGrabInteractable grab = food.GetComponent<XRGrabInteractable>();
        var interactor = grab.GetOldestInteractorSelecting();
        if (interactor != null)
            grab.interactionManager.SelectExit(interactor, grab);
        //Make it not a grab interactable anymore
        Destroy(grab);
        //Destroy all rigidbodies
        foreach (var rb in food.GetComponents<Rigidbody>())
            Destroy(rb);
        foreach (var col in food.GetComponents<Collider>())
            Destroy(col);
        Vector3 target = foodPoint.position;
        Vector3 start = food.transform.position;
        Vector3 rot = food.transform.eulerAngles;
        _anim.SetTrigger("Eat");
        float time = 0f;
        while (time < foodMovementTime)
        {
            yield return null;
            time += Time.deltaTime;
            float t = (time / foodMovementTime);
            Vector3 pos = Vector3.Slerp(start, target, t);
            food.transform.position = pos;
            food.transform.eulerAngles = Vector3.Slerp(rot, Vector3.zero, t);

        }
        yield return new WaitForSeconds(timeBeforeDestroyFood);
        StartCoroutine(_HeartRoutine());
        Destroy(food);
        yield return new WaitForSeconds(eatingCooldown);
        _canEat = true;
    }
    IEnumerator _HeartRoutine()
    {
        float time = 0f;
        float _lastHeart = _timeBetweenHearts;
        while (time < _heartTime)
        {
            yield return null;
            time += Time.deltaTime;
            _lastHeart += Time.deltaTime;
            if (_lastHeart >= _timeBetweenHearts) {
                GameObject heart = Instantiate(hearticles);
                Vector3 pos = _heartPos.position;
                pos.x += Random.Range(-1f * _heartRadius, _heartRadius);
                pos.z += Random.Range(-1f * _heartRadius, _heartRadius);
                heart.transform.position = pos;
                Destroy(heart, 3f);
                _lastHeart = 0f;
            }
        }
    }

    
}
