using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Bottom-left card before/between waves
public class NextWaveCard : MonoBehaviour
{
    public CanvasGroup group;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI hintText;
    public Image timerRadial;
    public TextMeshProUGUI timerText;
    public Transform chipParent;
    public CompositionChip chipTemplate;
    public Button startButton;
    public TextMeshProUGUI startLabel;
    public Button callNowButton;
    public TextMeshProUGUI callNowLabel;

    private GameController gc;
    private readonly List<CompositionChip> chips = new();
    private readonly List<(EnemyDefinition def, int count)> composition = new();
    private bool visible = true;
    private Coroutine fadeCo;

    private void Start()
    {
        gc = GameController.Instance;
        GameStrings s = GameStrings.Current;

        startLabel.text = s.startButton;
        hintText.text = s.startPrompt;
        startButton.onClick.AddListener(() => gc.StartGame());
        callNowButton.onClick.AddListener(() => gc.CallWaveEarly());

        gc.SetupStarted += OnSetupStarted;
        RebuildComposition();
    }

    private void OnDestroy()
    {
        if (gc != null) gc.SetupStarted -= OnSetupStarted;
    }

    private void OnSetupStarted(int wave) => RebuildComposition();

    private void Update()
    {
        GameStrings s = GameStrings.Current;
        bool preStart = !gc.HasGameStarted;
        bool setup = gc.HasGameStarted && !gc.HasWaveStarted && !gc.IsGameOver;
        SetVisible(preStart || setup);
        if (!visible) return;

        startButton.gameObject.SetActive(preStart);
        callNowButton.gameObject.SetActive(setup);
        hintText.gameObject.SetActive(preStart);

        int waveNo = Mathf.Min(gc.currentWave + 1, gc.TotalWaves);
        titleText.text = $"{s.nextWaveTitle}  ·  {string.Format(s.waveFormat, waveNo, gc.TotalWaves)}";

        float remaining = gc.spawner.SetupTimeRemaining;
        float duration = gc.spawner.SetupDuration;
        timerRadial.fillAmount = preStart ? 1f : (duration > 0f ? Mathf.Clamp01(remaining / duration) : 0f);
        timerText.text = preStart ? "--" : Mathf.CeilToInt(Mathf.Max(0f, remaining)).ToString();

        int bonus = gc.EarlyCallBonusPreview;
        callNowLabel.text = bonus > 0 ? $"{s.callNowButton}  +{bonus}" : s.callNowButton;
    }

    private void RebuildComposition()
    {
        gc.spawner.GetComposition(composition);
        while (chips.Count < composition.Count)
        {
            CompositionChip c = Instantiate(chipTemplate, chipParent);
            chips.Add(c);
        }
        for (int i = 0; i < chips.Count; i++)
        {
            bool used = i < composition.Count;
            chips[i].gameObject.SetActive(used);
            if (used) chips[i].Bind(composition[i].def, composition[i].count);
        }
    }

    private void SetVisible(bool v)
    {
        if (v == visible) return;
        visible = v;
        if (fadeCo != null) StopCoroutine(fadeCo);
        fadeCo = UITween.Fade(this, group, v ? 1f : 0f, 0.2f);
    }
}
