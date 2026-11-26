using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Before/between waves
public class NextWaveCard : MonoBehaviour
{
    public CanvasGroup group;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI timerText;
    [Tooltip("Image Type = Filled, Fill Method = Horizontal. Drains as the setup time runs out.")]
    public RectTransform timerFill;
    public Transform pillParent;
    public FactionPill pillTemplate;
    [Tooltip("Only shown before the first wave.")]
    public Button startButton;
    public TextMeshProUGUI startLabel;

    private GameController gc;
    private readonly List<FactionPill> pills = new();
    private readonly List<(EnemyDefinition def, int count)> composition = new();
    private bool visible = true;

    private void Start()
    {
        gc = GameController.Instance;
        startLabel.text = GameStrings.Current.startButton;
        startButton.onClick.AddListener(() => gc.StartGame());
        gc.SetupStarted += OnSetupStarted;
        Rebuild();
    }

    private void OnDestroy()
    {
        if (gc != null) gc.SetupStarted -= OnSetupStarted;
    }

    // A new wave is coming: refresh the tiles and give the card a little pop
    private void OnSetupStarted(int wave)
    {
        Rebuild();
        transform.DOKill(true);
        transform.DOPunchScale(Vector3.one * 0.05f, 0.25f, 1, 0f).SetUpdate(true);
    }

    private void Update()
    {
        bool preStart = !gc.HasGameStarted;
        bool setup = gc.HasGameStarted && !gc.HasWaveStarted && !gc.IsGameOver;
        SetVisible(preStart || setup);
        if (!visible) return;

        startButton.gameObject.SetActive(preStart);

        int waveNo = Mathf.Min(gc.currentWave + 1, gc.TotalWaves);
        titleText.text = string.Format(GameStrings.Current.waveFormat, waveNo, gc.TotalWaves);

        float remaining = Mathf.Max(0f, gc.spawner.SetupTimeRemaining);
        float duration = gc.spawner.SetupDuration;
        timerText.text = preStart ? "" : $"{Mathf.CeilToInt(remaining)}s";
        float k = preStart ? 1f : (duration > 0f ? Mathf.Clamp01(remaining / duration) : 0f);
        timerFill.anchorMax = new Vector2(k, 1f);
    }

    // One tile per AI company, biggest first
    private void Rebuild()
    {
        gc.spawner.GetComposition(composition);
        var byFaction = composition
            .GroupBy(c => c.def.faction)
            .Select(g => (faction: g.Key, count: g.Sum(c => c.count)))
            .OrderByDescending(x => x.count)
            .ToList();

        while (pills.Count < byFaction.Count)
            pills.Add(Instantiate(pillTemplate, pillParent));

        for (int i = 0; i < pills.Count; i++)
        {
            bool used = i < byFaction.Count;
            pills[i].gameObject.SetActive(used);
            if (used) pills[i].Bind(byFaction[i].faction, byFaction[i].count);
        }
    }

    private void SetVisible(bool v)
    {
        if (v == visible) return;
        visible = v;
        group.DOKill();
        group.interactable = v;
        group.blocksRaycasts = v;
        group.DOFade(v ? 1f : 0f, 0.2f).SetUpdate(true);
    }
}
