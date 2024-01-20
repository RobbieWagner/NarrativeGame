using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class SceneTransition : MonoBehaviour
{
    public Image screenCover;
    public bool screenCoveredOnStart = false;

    public Color activeColor;

    public float defaultFadeTime = 3f;
    public Ease ease;

    public static SceneTransition Instance {get; private set;}

    private void Awake()
    {
        if (Instance != null && Instance != this) 
        { 
            Destroy(gameObject); 
        } 
        else 
        { 
            Instance = this; 
        } 

        screenCover.color = activeColor;
        if(screenCoveredOnStart) screenCover.enabled = true;
    }

    public IEnumerator FadeOutScreen(float fadeTime = -1f)
    {
        screenCover.enabled = true;
        screenCover.color = activeColor;

        float waitTime = .25f;
        float fadeOutTime = fadeTime - waitTime;

        if(fadeTime < 0f) fadeOutTime = defaultFadeTime - waitTime;
        yield return new WaitForSeconds(waitTime);
        yield return screenCover.DOColor(Color.clear, fadeOutTime).SetEase(ease).WaitForCompletion();

        screenCover.enabled = false;

        StopCoroutine(FadeOutScreen(fadeTime));
    }

    public IEnumerator FadeInScreen(float fadeTime = -1f)
    {
        screenCover.color = Color.clear;
        screenCover.enabled = true;

        float fadeInTime = fadeTime;

        if(fadeTime < 0f) fadeInTime = defaultFadeTime;
        yield return screenCover.DOColor(Color.clear, fadeInTime).SetEase(ease).WaitForCompletion();

        StopCoroutine(FadeInScreen(fadeTime));
    }


}
