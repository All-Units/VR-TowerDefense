using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetonateCastle : MonoBehaviour
{
    [SerializeField] Transform _castleRoot;
    [SerializeField] float _timeToDestroyCastle = 7f;
    [SerializeField] Vector3 localRotateTarget = new Vector3(20f, 0f, 0f);
    [SerializeField] Vector3 localPosTarget = new Vector3(0f, -7f, 0f);
    public List<MeshRenderer> _castleRenderers = new List<MeshRenderer>();    
    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        GameStateManager.instance.OnGameLose += Detonate;
    }
    static DetonateCastle instance;

    // Update is called once per frame
    void Update()
    {
        
    }
    public static void DetonateStatic()
    {
        instance.Detonate();
    }
    public void Detonate()
    {
        //_castleRenderers = _castleRoot.GetComponentsInChildren<MeshRenderer>().ToList();
        //_castleRenderers = _castleRenderers.OrderBy(x => x.transform.position.y * -1f).
        //    ThenBy(x => x.transform.position.x).ToList();
        StartCoroutine(_CastleDetonationRoutine());
        StartCoroutine(_MoveExplodingCastle());
    }
    IEnumerator _CastleDetonationRoutine()
    {
        float timeBetween = _timeToDestroyCastle / (float)_castleRenderers.Count;
        float t = 0f;
        foreach (var renderer in _castleRenderers)
        {
            try
            {
                renderer.enabled = false;
                foreach (var col in renderer.GetComponents<Collider>())
                    col.enabled = false;
            }
            catch (MissingReferenceException e) { continue; }
            yield return new WaitForSeconds(timeBetween);
            for (int i = 0; i < 3; i++)
            {
                _SpawnExplosionAt(renderer.transform.position + Random.insideUnitSphere * 2f);
            }
            

        }
        
    }
    IEnumerator _MoveExplodingCastle()
    {
        yield return new WaitForSeconds(_timeToDestroyCastle / 3f);
        Vector3 startRot = _castleRoot.transform.localEulerAngles;
        Vector3 targetRot = startRot + localRotateTarget;
        Vector3 startPos = _castleRoot.transform.localPosition;
        Vector3 targetPos = startPos + localPosTarget;
        float time = 0f;
        while (time <= _timeToDestroyCastle)
        {
            float t = time / _timeToDestroyCastle;
            _castleRoot.transform.localEulerAngles = Vector3.Slerp(startRot, targetRot, t);
            _castleRoot.transform.localPosition = Vector3.Slerp(startPos, targetPos, t);
            yield return null;
            time += Time.deltaTime;
        }
        //yield break;
    }
    void _SpawnExplosionAt(Vector3 pos)
    {
        GameObject explosion = Instantiate(Resources.Load<GameObject>("Prefabs/explosion"));
        explosion.DestroyAfter(5f);
        explosion.transform.position = pos;
    }
}
