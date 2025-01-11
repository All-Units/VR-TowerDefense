using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StartGamePanel : MonoBehaviour
{
    [SerializeField] private Transform canvasTransform;
    [SerializeField]
    private float distanceFromPlayer = 3f;
    [SerializeField]
    private float height = 0.5f;
    [SerializeField] float startDelay = 1f;
    [SerializeField] float growTime = 0.6f;
    [SerializeField] Color countColor;
    [SerializeField] TextMeshProUGUI portalCountLabel;
    [SerializeField] TextMeshProUGUI waveCountLabel;
    [SerializeField] TextMeshProUGUI MapNameLabel;
    public static string MapName = "";
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(_StartRoutine());
    }
    IEnumerator _StartRoutine()
    {
        canvasTransform.gameObject.SetActive(false);
        yield return new WaitForSeconds(startDelay);
        Vector3 scaleTarget = canvasTransform.localScale;
        canvasTransform.localScale = Vector3.zero;
        float time = 0;
        if (EnemyManager.instance && EnemyManager.instance.IS_TUTORIAL) yield break;
        _RepositionPanel();
        _startPos = cam.position;
        _FillLabels();
        canvasTransform.gameObject.SetActive(true);
        while (time <= growTime)
        {
            float t = time / growTime;
            Vector3 scale = Vector3.Lerp(Vector3.zero, scaleTarget, t);
            canvasTransform.localScale = scale;
            yield return null;
            time += Time.deltaTime;
        }
        canvasTransform.localScale = scaleTarget;
        _waiter = _WaitForMove();
        StartCoroutine(_waiter);

    }
    public void Close()
    {
        if (_waiter != null)
            StopCoroutine(_waiter);
        StartCoroutine(_HideRoutine());
    }
    IEnumerator _waiter;
    IEnumerator _WaitForMove()
    {
        while (true)
        {
            float distance = Vector3.Distance(cam.position, _startPos);
            if (distance >= 1f)
            {
                StartCoroutine(_HideRoutine());
                yield break;
            }
            yield return null;
        }
    }
    IEnumerator _HideRoutine()
    {
        Vector3 scaleStart = canvasTransform.localScale;
        canvasTransform.localScale = Vector3.zero;
        float time = 0;
        
        while (time <= growTime)
        {
            float t = time / growTime;
            Vector3 scale = Vector3.Lerp(scaleStart, Vector3.zero, t);
            canvasTransform.localScale = scale;
            yield return null;
            time += Time.deltaTime;
        }
        canvasTransform.localScale = Vector3.zero;
    }
    Vector3 _startPos;
    void _FillLabels()
    {
        string waveCount = EnemyManager.WaveCountTotal.ToString().ColorString(countColor);
        string portalCount = $"{EnemyManager.instance.SpawnPoints.Count}".ColorString(countColor);

        portalCountLabel.text = $"Portals: {portalCount}";
        waveCountLabel.text = $"Waves: {waveCount}";

        if (MapName != "")
            MapNameLabel.text = MapName;

    }

    // Update is called once per frame
    void Update()
    {
        
    }
    Transform cam => InventoryManager.instance.playerCameraTransform;
    private void _RepositionPanel()
    {
        if (InventoryManager.instance == null) return;

        transform.position = cam.position;

        Vector3 angle = new Vector3(0f, cam.eulerAngles.y - 90f, 0f);
        transform.eulerAngles = angle;
        canvasTransform.localPosition = new Vector3(distanceFromPlayer, height, 0f);

        Vector3 canvasPos = canvasTransform.position;
        Vector3 center = canvasPos + Vector3.up * 100f;
        RaycastHit hit;
        LayerMask mask = LayerMask.GetMask("Ground");
        if (Physics.Raycast(center, Vector3.down, out hit, float.PositiveInfinity, mask))
        {
            //print($"Hit y level {hit.point.y}, canvas currently at {canvasPos.y}");
            if (hit.point.y > canvasPos.y)
            {
                float offset = Mathf.Max(height, 0.3f);
                canvasPos.y = hit.point.y + offset;
                canvasTransform.position = canvasPos;
                //print($"Canvas was too low, moving up to {canvasTransform.position.y}");
            }
        }
    }
}
