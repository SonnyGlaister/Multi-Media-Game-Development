using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillTreeMenu : MonoBehaviour
{
    [Serializable]
    public struct Layout
    {
        public Vector2 anchor;
        public Vector2 pivot;
        public Vector2 position;
        public float scale;
    }

    public static SkillTreeMenu Instance { get; private set; }
    public event Action<bool> OpenChanged;

    [Header("References")]
    public RectTransform window;
    public CanvasGroup windowGroup;
    public Button expandButton;
    public CanvasGroup contentGroup;
    public ScrollRect scroll;
    public CanvasGroup backdrop;
    public Button backdropButton;
    public Button closeButton;
    public TextMeshProUGUI titleText;

    [Header("Layouts")]
    public Layout mini = new Layout { anchor = new Vector2(1f, 0f), pivot = new Vector2(1f, 0f), position = new Vector2(-24f, 24f), scale = 0.22f };
    public Layout full = new Layout { anchor = new Vector2(0.5f, 0.5f), pivot = new Vector2(0.5f, 0.5f), position = Vector2.zero, scale = 1f };
    public float tweenTime = 0.2f;
    public KeyCode toggleKey = KeyCode.T;

    public bool IsOpen { get; private set; }

    private GameController gc;
    private Coroutine moveCo, dimCo;

    private void Awake()
    {
        Instance = this;
        Apply(mini);
        SetGroup(windowGroup, false);
        SetGroup(backdrop, false);
        SetContentInteractive(false);
    }

    private void Start()
    {
        gc = GameController.Instance;
        gc.SetupStarted += OnSetupStarted;
        gc.GameEnded += OnGameEnded;

        expandButton.onClick.AddListener(Open);
        closeButton.onClick.AddListener(Close);
        backdropButton.onClick.AddListener(Close);
        if (titleText != null) titleText.text = GameStrings.Current.skillTreeTitle;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        if (gc == null) return;
        gc.SetupStarted -= OnSetupStarted;
        gc.GameEnded -= OnGameEnded;
    }

    private void OnSetupStarted(int wave)
    {
        if (windowGroup.alpha < 1f) UITween.Fade(this, windowGroup, 1f);
    }

    private void OnGameEnded(bool won)
    {
        Close();
        UITween.Fade(this, windowGroup, 0f);
    }

    private void Update()
    {
        if (gc == null || !gc.HasGameStarted || gc.IsGameOver) return;
        if (Input.GetKeyDown(toggleKey)) Toggle();
        else if (IsOpen && Input.GetKeyDown(KeyCode.Escape)) Close();
    }

    public void Toggle() { if (IsOpen) Close(); else Open(); }
    public void Open() => SetOpen(true);
    public void Close() => SetOpen(false);

    private void SetOpen(bool open)
    {
        if (open == IsOpen) return;
        if (open && (gc == null || !gc.HasGameStarted || gc.IsGameOver)) return;
        IsOpen = open;

        expandButton.interactable = !open;
        SetContentInteractive(open);

        if (dimCo != null) StopCoroutine(dimCo);
        dimCo = UITween.Fade(this, backdrop, open ? 1f : 0f);

        if (moveCo != null) StopCoroutine(moveCo);
        Layout from = Capture(), to = open ? full : mini;
        moveCo = StartCoroutine(UITween.Run(tweenTime, e => Apply(Lerp(from, to, e))));

        OpenChanged?.Invoke(open);
    }

    // When mini, clicks pass through the perks to the expand button.
    private void SetContentInteractive(bool on)
    {
        contentGroup.interactable = on;
        contentGroup.blocksRaycasts = on;
        scroll.enabled = on;
    }

    private static void SetGroup(CanvasGroup g, bool on)
    {
        g.alpha = on ? 1f : 0f;
        g.interactable = on;
        g.blocksRaycasts = on;
    }

    private Layout Capture() => new Layout
    {
        anchor = window.anchorMin,
        pivot = window.pivot,
        position = window.anchoredPosition,
        scale = window.localScale.x
    };

    private void Apply(Layout l)
    {
        window.anchorMin = window.anchorMax = l.anchor;
        window.pivot = l.pivot;
        window.anchoredPosition = l.position;
        window.localScale = Vector3.one * l.scale;
    }

    private static Layout Lerp(Layout a, Layout b, float t) => new Layout
    {
        anchor = Vector2.Lerp(a.anchor, b.anchor, t),
        pivot = Vector2.Lerp(a.pivot, b.pivot, t),
        position = Vector2.Lerp(a.position, b.position, t),
        scale = Mathf.Lerp(a.scale, b.scale, t)
    };
}