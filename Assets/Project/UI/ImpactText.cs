using Codice.CM.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class ImpactText : MonoBehaviour
{
    [Header("Gameplay variables")]
    [SerializeField] AnimationCurve sizeCurve;
    [SerializeField] float _displayTime = 1f;
    [SerializeField] float amplitude = 1f;
    [SerializeField] float frequency = 2f;
    [SerializeField] float frequencyVariance = 0.3f;
    [SerializeField] float moveSpeed = 1f;
    public List<_ColorsByType> _colors = new List<_ColorsByType>();

    [Header("References")]
    [SerializeField] TextMeshProUGUI displayText;
    Transform displayParent => displayText.transform.parent;

    public void InitText(string text)
    {
        displayText.text = text;
    }
    Vector3 startPos;
    // Start is called before the first frame update
    void Start()
    {
        startPos = transform.position;
        StartCoroutine(_SizeRoutine());
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    IEnumerator _SizeRoutine()
    {
        float time = 0f;
        transform.position = startPos;
        Vector3 localStart = transform.localPosition;
        float offset = Random.value;
        while (time <= _displayTime) 
        {
            if (XRPauseMenu.IsPaused)
            {
                
                yield return null;
                continue;
            }
            time += Time.deltaTime;

            Vector3 scale = Vector3.one * sizeCurve.Evaluate(time / _displayTime);
            displayParent.localScale = scale;
            
            Vector3 pos = transform.localPosition;
            pos.x = localStart.x + Mathf.Sin(Time.time * frequency + offset) * amplitude;
            pos.y += moveSpeed * Time.deltaTime;
            
            transform.localPosition = pos;

            yield return null;
        }

        //TODO remove recursion
        //StartCoroutine(_SizeRoutine());   
    }
    [Serializable]
    public struct _ColorsByType
    {
        public Color c;
        public _ImpactTypes type;
    }
    public enum _ImpactTypes
    {
        Damage,
        Kill,
        TowerDamage,
    }

    public static void ImpactTextAt(Vector3 pos, string text, _ImpactTypes type, float scale = 1f)
    {
        GameObject go = Instantiate(Resources.Load<GameObject>("Prefabs/ImpactText"));
        go.transform.position = pos;
        go.transform.localScale *= scale;

        ImpactText impactText = go.GetComponent<ImpactText>();
        float variance = impactText.frequencyVariance;
        impactText.frequency += Random.Range(variance * -1, variance);
        impactText.displayText.text = text;
        impactText.displayText.color = impactText._colors.Find(x => x.type == type).c;
        go.DestroyAfter(impactText._displayTime + 0.1f);
    }
}
