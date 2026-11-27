using System;
using TMPro;
using UnityEngine;
using DG.Tweening;

public class FloatingText : MonoBehaviour
{
    public TextMeshProUGUI label;
    public float rise = 50f;
    public float duration = 0.8f;

    private Vector2 startScreen;
    private Vector3? world;
    private float t;
    private Color color;
    private Camera cam;
    private Action<FloatingText> onDone;

    public void Play(string text, Color c, Vector2 screen, Vector3? worldAnchor, float scale, Action<FloatingText> done)
    {
        label.text = text;
        color = c;
        startScreen = screen;
        world = worldAnchor;
        t = 0f;
        onDone = done;
        cam = Camera.main;
        transform.localScale = Vector3.one * scale;
        gameObject.SetActive(true);
        Tick();
    }

    private void Update() => Tick();

    private void Tick()
    {
        t += Time.unscaledDeltaTime;
        float k = Mathf.Clamp01(t / duration);

        Vector2 basePos = startScreen;
        if (world.HasValue && cam != null)
        {
            Vector3 sp = cam.WorldToScreenPoint(world.Value);
            if (sp.z < 0f) { Finish(); return; }
            basePos = sp;
        }
        transform.position = basePos + Vector2.up * rise * DOVirtual.EasedValue(0f, 1f, k, Ease.OutCubic);

        Color c = color;
        c.a = 1f - Mathf.Clamp01((k - 0.5f) * 2f);
        label.color = c;

        if (k >= 1f) Finish();
    }

    private void Finish()
    {
        gameObject.SetActive(false);
        onDone?.Invoke(this);
    }
}
