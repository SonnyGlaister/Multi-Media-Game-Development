using System.Collections.Generic;
using UnityEngine;

// Stacked messages, newest on top
public class ToastManager : MonoBehaviour
{
    public ToastView toastTemplate;
    public Transform stackParent;
    public int maxToasts = 3;
    public float duration = 2.6f;

    private readonly List<ToastView> active = new();
    private GameController gc;

    private void Start()
    {
        gc = GameController.Instance;
        if (gc != null) gc.MessageRequested += Show;
    }

    private void OnDestroy()
    {
        if (gc != null) gc.MessageRequested -= Show;
    }

    public void Show(string message, MessageType type)
    {
        foreach (ToastView t in active)
        {
            if (t.Message == message) { t.Repeat(duration); return; }
        }

        if (active.Count >= maxToasts)
        {
            active[0].Dismiss();
            active.RemoveAt(0);
        }

        ToastView view = Instantiate(toastTemplate, stackParent);
        view.gameObject.SetActive(true);
        view.transform.SetAsFirstSibling();
        view.Play(message, type, duration, () => active.Remove(view));
        active.Add(view);
    }
}
