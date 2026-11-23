using System;
using System.Collections;
using Unit404.Integrante4.Core;
using UnityEngine;
using UnityEngine.UI;

// Full-screen fade tinted with the next culture's accent, played before the loading scene.
public class CultureTransition : MonoBehaviour
{
    public Image overlay;
    public float seconds = 0.8f;

    void Awake()
    {
        if (overlay == null) return;
        overlay.raycastTarget = false;
        overlay.gameObject.SetActive(false);
    }

    public void Play(int finishedLevel, Action onDone)
    {
        if (overlay == null) { onDone?.Invoke(); return; }
        StopAllCoroutines();
        StartCoroutine(Run(finishedLevel, onDone));
    }

    IEnumerator Run(int finishedLevel, Action onDone)
    {
        ColorUtility.TryParseHtmlString(CulturePalette.TransitionHex(finishedLevel), out Color tint);
        overlay.gameObject.SetActive(true);
        overlay.raycastTarget = true; // block clicks while fading
        for (float t = 0f; t < seconds; t += Time.unscaledDeltaTime)
        {
            overlay.color = new Color(tint.r, tint.g, tint.b, UiAnimation.FadeAlpha(t, seconds));
            yield return null;
        }
        overlay.color = new Color(tint.r, tint.g, tint.b, 1f);
        onDone?.Invoke();
    }
}
