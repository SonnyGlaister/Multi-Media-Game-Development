using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public static class UITween
{
    public static float EaseOut(float t) => 1f - Mathf.Pow(1f - t, 3f);

    public static IEnumerator Run(float duration, Action<float> step)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            step(EaseOut(Mathf.Clamp01(t / duration)));
            yield return null;
        }
        step(1f);
    }

    public static Coroutine Fade(MonoBehaviour host, CanvasGroup group, float to, float duration = 0.18f)
    {
        bool on = to > 0.5f;
        group.interactable = on;
        group.blocksRaycasts = on;
        float from = group.alpha;
        return host.StartCoroutine(Run(duration, e => group.alpha = Mathf.Lerp(from, to, e)));
    }

    public static Coroutine Punch(MonoBehaviour host, Transform target, float amount = 0.15f, float duration = 0.25f)
    {
        return host.StartCoroutine(Run(duration, e => target.localScale = Vector3.one * (1f + amount * Mathf.Sin(e * Mathf.PI))));
    }

    // Shake around a fixed origin and return to it.
    public static Coroutine Shake(MonoBehaviour host, RectTransform target, Vector2 origin, float strength = 8f, float duration = 0.25f)
    {
        return host.StartCoroutine(Run(duration, e =>
        {
            float s = (1f - e) * strength;
            target.anchoredPosition = e < 1f ? origin + UnityEngine.Random.insideUnitCircle * s : origin;
        }));
    }

    public static Coroutine CountTo(MonoBehaviour host, int from, int to, float duration, Action<int> set)
    {
        return host.StartCoroutine(Run(duration, e => set(Mathf.RoundToInt(Mathf.Lerp(from, to, e)))));
    }

    public static Coroutine Flash(MonoBehaviour host, Graphic graphic, Color flash, Color settle, float duration = 0.3f)
    {
        return host.StartCoroutine(Run(duration, e => graphic.color = Color.Lerp(flash, settle, e)));
    }
}