using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class GuidedMissileTargeter : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The reference to the action to confirm tower takeover selection.")]
    private InputActionReference targetActionReference;

    [SerializeField] private GameObject castPoint;
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private TargetVFXController targetVFXPrefab;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private float radius = 2;
    [SerializeField] private float length = 50;
    [SerializeField] private int totalTargets = 1;
    
    private bool isTargeting;

    public List<Enemy> targets = new();
    XRGrabInteractable grab;

    public void CapNumberOfTargets(int cap)
    {
        targets.RemoveAll(e => e == null);
        if (cap >= targets.Count) return;
        
        int toCull = targets.Count - cap;
        int culled = 0;
        for (int i = targets.Count - 1; culled < toCull; i--)
        {
            var target = targets[i];
            var vfx = target.GetComponentInChildren<TargetVFXController>();
            if (vfx != null)
            {
                Destroy(vfx.gameObject);
            }
            targets.RemoveAt(i);
            culled++;
        }
    }
    public Enemy GetEnemy(int idx)
    {
        targets.RemoveAll(e=> e == null);

        if (idx == -1)
        {
            if (targets.Count == 0) return null;
            return targets.GetRandom();
        }
            
        return targets.Count >= 1 ? targets[idx % targets.Count] : null;
    }
    
    private void Start()
    {
        var targetAction = Utilities.GetInputAction(targetActionReference);
        if (targetAction != null)
        {
            //targetAction.started += TargetActionOnStarted;
            //targetAction.canceled += TargetActionOnCanceled;
        }

        Enemy.OnDeath += OnEnemyDeath;
        _castRay = new Ray(castPoint.transform.position, castPoint.transform.forward);
        grab = GetComponent<XRGrabInteractable>();
        //grab.act
        grab.selectExited.AddListener(OnDrop);
        grab.activated.AddListener(_TriggerPulled);
        grab.deactivated.AddListener(_TriggerReleased);
    }
    void _TriggerPulled(ActivateEventArgs a)
    {
        TargetActionOnStarted(new InputAction.CallbackContext());
    }
    void _TriggerReleased(DeactivateEventArgs a)
    {
        TargetActionOnCanceled(new InputAction.CallbackContext());
    }
    void OnDrop(SelectExitEventArgs e)
    {
        foreach (var target in targets)
        {
            var vfx = target.GetComponentInChildren<TargetVFXController>();
            if (vfx)
                Destroy(vfx.gameObject);
        }
        targets.Clear();
        isTargeting = false;

    }
    public void OnEnemyDeath(Enemy obj)
    {
        targets.Remove(obj);
        var vfx = obj.GetComponentInChildren<TargetVFXController>();
        if(vfx)
            Destroy(vfx.gameObject);
    }

    [SerializeField] private float scanRate = .25f;
    private float _currentRate = 0;
    private void Update()
    {
        if (isTargeting)
        {
            ScanForTargets();
        }
        else
        {
            Vector3 low = new Vector3(0f, -1000f, 0f);
            lineRenderer.SetPosition(0, low);
            lineRenderer.SetPosition(1, low);
        }

    }

    private void ScanForTargets()
    {
        _castRay = new Ray(transform.position, transform.forward);

        _currentRate += Time.deltaTime;

        if (_currentRate >= scanRate)
        {
            ScanForEnemies();
            _currentRate = 0;
        }

        DrawLine();
    }

    

    public bool IsTargeting()
    {
        return targets.Any();
    }

    private Ray _castRay;
    private float t = 0;
    private void ScanForEnemies()
    {
        if (Physics.SphereCast(_castRay,radius, out var hit,length, layerMask))
        {
            if (hit.transform.TryGetComponent(out Enemy e) && !targets.Contains(e))
            {
                //Debug.Log($"Enemy {e.gameObject} targeted");
                TargetVFXController oldVFX = null;
                if(targets.Count == totalTargets)
                {
                    if(targets[0])
                        oldVFX = targets[0].GetComponentInChildren<TargetVFXController>();
                    targets.RemoveAt(0);
                }   
                
                targets.Add(e);
                Transform parent = e.transform.Find("targetingRune");
                if (parent == null)
                    parent = e.transform;
                if (oldVFX)
                {
                    oldVFX.transform.SetParent(parent);
                }
                else
                {
                    oldVFX = Instantiate(targetVFXPrefab, parent);
                }
                
                oldVFX.transform.localPosition = Vector3.zero;
                oldVFX.transform.localScale = Vector3.one;
            }
        }
        
    }
    
    

    private void OnDrawGizmos()
    {
        DrawGizmoViz();
    }

    private void DrawGizmoViz()
    {
        t += Time.deltaTime;
        if (t > 1)
            t = 0;
        Gizmos.DrawWireSphere(_castRay.GetPoint(Mathf.Lerp(0, length, t)), radius);
    }

    private void DrawLine()
    {
        lineRenderer.SetPosition(0, _castRay.GetPoint(0));
        lineRenderer.SetPosition(1, _castRay.GetPoint(50));
    }

    private void TargetActionOnCanceled(InputAction.CallbackContext obj)
    {
        isTargeting = false;
    }

    private void TargetActionOnStarted(InputAction.CallbackContext obj)
    {
        isTargeting = true;
    }
}