using System;
using DG.Tweening;
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
        group.DOFade(1f, 0.15f).SetUpdate(true);
    }

    public void Repeat(float duration)
    {
        count++;
        label.text = $"{Message}  <color=#{UITheme.Hex(UITheme.Current.textMuted)}>×{count}</color>";
        remaining = duration;
        transform.DOKill(true);
        transform.DOPunchScale(Vector3.one * 0.05f, 0.15f, 1, 0f).SetUpdate(true);
    }

    public void Dismiss()
    {
        if (dismissing) return;
        dismissing = true;
        group.DOKill();
        group.DOFade(0f, 0.2f).SetUpdate(true).OnComplete(() =>
        {
            onDone?.Invoke();
            Destroy(gameObject);
        });
    }

    private void Update()
    {
        if (dismissing) return;
        remaining -= Time.unscaledDeltaTime;
        if (remaining <= 0f) Dismiss();
    }
}
