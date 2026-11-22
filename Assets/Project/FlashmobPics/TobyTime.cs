using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

[ExecuteInEditMode]
public class TobyTime : MonoBehaviour
{
    [Tooltip("How much faster than real time the clock should run")]
    [SerializeField] float timeScalar = 10f;

    [SerializeField] Transform secondHand;
    [SerializeField] Transform minuteHand;
    [SerializeField] Transform hourHand;

    // Sixty seconds in a minute, sixty minutes in an hour, twelve hours on a clock
    [SerializeField] float twelveHours = 60f * 60f * 12f;
    [SerializeField] float currentTime;

    [SerializeField] GameObject picturePrefab;
    VideoPlayer player;
    HashSet<long> ozFrames = new HashSet<long>() 
        {0,1,
        11, 12, 13, 14, 15, 16, 17,
        27, 28, 29, 30, 31
        };

    [SerializeField] List<Sprite> ozSprites = new List<Sprite>();
    [SerializeField] List<Sprite> tobySprites = new List<Sprite>();
    // Start is called before the first frame update
    void Start()
    {
        if (Application.isPlaying == false)
        {
            return;
        }
        if (player == null)
        {
            player = GetComponentInChildren<VideoPlayer>();
        }
        _initPictures();
    }
    List<Image> renderers = new List<Image>();
    void _initPictures()
    {
        renderers.Add(picturePrefab.GetComponentInChildren<Image>());
        for (int i = 1; i < 12; i++)
        {
            GameObject pic = Instantiate(picturePrefab, picturePrefab.transform.parent);
            Image renderer = pic.GetComponentInChildren<Image>();
            pic.transform.localEulerAngles = new Vector3(0f, 0f, 30f * i);
            renderer.transform.localEulerAngles = new Vector3(0f, 0f, 30f * i);
            renderers.Add(renderer);
        }
    }
    void _setPictures(whichCat cat)
    {
        List<Sprite> sprites = (cat == whichCat.Toby) ? tobySprites : ozSprites;
        for (int i = 0; i < 12; i++)
        {
            Image r = renderers[i];
            Sprite media = sprites[i];

            r.sprite = media;
            float h = (float)media.rect.height;
            float w = (float)media.rect.width;
            float ratio = Math.Min(h, w) / Math.Max(h, w);
            r.transform.localScale = Vector3.one;
            //If TALLER than WIDE, make X small (skinny)
            if (h > w){
                r.transform.localScale = new Vector3(ratio, 1f, 1f);
            }

            //Else if WIDER than TALL, make Z small (portrait orientation)
            else if (w > h){
                r.transform.localScale = new Vector3(1f, ratio, 1f);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        updateHands();
        updatePictures();

    }
    float seconds;
    float minutes;
    float hours;

    enum whichCat
    {
        Toby,
        Oz,
        Neither
    }
    whichCat lastCat = whichCat.Neither;

    void updatePictures()
    {
         if (Application.isPlaying == false)
        {
            return;
        }
        whichCat cat = ozFrames.Contains(player.frame) ?  whichCat.Oz : whichCat.Toby;
        if (cat != lastCat)
        {
            _setPictures(cat);
        }
        lastCat = cat;
    }


    void updateHands()
    {
        currentTime += Time.deltaTime * timeScalar;
        // Loop back around if exceeds twelve hours
        currentTime %= twelveHours;
        seconds = currentTime % 60f;
        seconds = Mathf.Lerp(0f, 360f, seconds / 60f);
        minutes = currentTime % (60f * 60f);
        minutes /= 60f;
        minutes = Mathf.Lerp(0f, 360f, minutes / 60f);
        hours = currentTime / (60f * 60f);
        hours = Mathf.Lerp(0f, 360f, hours / 12f);
        // Rotate Y 
        secondHand.localEulerAngles = new Vector3(0f, seconds, 0f);
        minuteHand.localEulerAngles = new Vector3(0f, minutes, 0f);
        hourHand.localEulerAngles = new Vector3(0f, hours, 0f);

    }
}
