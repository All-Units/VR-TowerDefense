using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeScreen : MonoBehaviour
{
    public static FadeScreen instance;
    public bool fadeOnStart = true;
    public float fadeDuration = 2;
    public Color fadeColor;
    public AnimationCurve fadeCurve;
    public string colorPropertyName = "_Color";
    private Renderer rend;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        rend = GetComponent<Renderer>();
        rend.enabled = false;

        if (fadeOnStart)
            FadeIn();
    }

    public void SetFadeInstant(float a)
    {
        rend.enabled = true;
        Color newColor = fadeColor;
        newColor.a = Mathf.Clamp01(a);
        rend.material.SetColor(colorPropertyName, newColor);
    }

    /// <summary>
    /// Starts black, ends see through
    /// </summary>
    public void FadeIn()
    {
        Fade(1, 0);
    }
    public static void Fade_Out(float t = -1)
    {
        instance.StartCoroutine(instance.FadeRoutine(0, 1, t));
        //instance.FadeOut();
    }
    /// <summary>
    /// Starts see through, ends black
    /// </summary>
    public void FadeOut()
    {
        Fade(0, 1);
    }

    public void Fade(float alphaIn, float alphaOut)
    {
        StartCoroutine(FadeRoutine(alphaIn,alphaOut));
    }

    public IEnumerator FadeRoutine(float alphaIn,float alphaOut, float t = -1)
    {
        rend.enabled = true;
        if (t == -1)
            t = fadeDuration;
        float timer = 0;
        while(timer <= t)
        {
            Color newColor = fadeColor;
            newColor.a = Mathf.Lerp(alphaIn, alphaOut, fadeCurve.Evaluate(timer / t));

            rend.material.SetColor(colorPropertyName, newColor);

            timer += Time.deltaTime;
            yield return null;
        }

        Color newColor2 = fadeColor;
        newColor2.a = alphaOut;
        rend.material.SetColor(colorPropertyName, newColor2);

        if(alphaOut == 0)
            rend.enabled = false;
    }
}
