using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ToastView : MonoBehaviour
{
    public CanvasGroup group;
    public Image accent;
    public TextMeshProUGUI label;

    public string Message { get; private set; }

    private int count = 1;
    private float remaining;
    private bool dismissing;
    private Action onDone;

    public void Play(string message, MessageType type, float duration, Action done)
    {
        Message = message;
        onDone = done;
        remaining = duration;
        if (accent != null) accent.color = UITheme.Current.MessageColor(type);
        label.text = message;
        group.alpha = 0f;
        StartCoroutine(UITween.Run(0.15f, e => group.alpha = e));
    }

    public void Repeat(float duration)
    {
        count++;
        label.text = $"{Message}  <color=#{UITheme.Hex(UITheme.Current.textMuted)}>×{count}</color>";
        remaining = duration;
        UITween.Punch(this, transform, 0.05f, 0.15f);
    }

    public void Dismiss()
    {
        if (dismissing) return;
        dismissing = true;
        StartCoroutine(Out());
    }

    private void Update()
    {
        if (dismissing) return;
        remaining -= Time.unscaledDeltaTime;
        if (remaining <= 0f) Dismiss();
    }

    private IEnumerator Out()
    {
        yield return UITween.Run(0.2f, e => group.alpha = 1f - e);
        onDone?.Invoke();
        Destroy(gameObject);
    }
}
