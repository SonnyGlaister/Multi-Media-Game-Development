using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

// Victory/defeat modal with post-game stats/options.
public class GameOverScreen : MonoBehaviour
{
    public CanvasGroup group;
    public RectTransform card;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI subtitleText;
    public TextMeshProUGUI statsText;
    public TextMeshProUGUI factionText;
    public Button retryButton;
    public Button nextButton;
    public Button menuButton;

    private GameController gc;

    private void Start()
    {
        gc = GameController.Instance;
        group.alpha = 0f;
        group.blocksRaycasts = false;
        group.interactable = false;

        gc.GameEnded += Show;
        retryButton.onClick.AddListener(gc.RestartLevel);
        menuButton.onClick.AddListener(gc.GoToMainMenu);
        nextButton.onClick.AddListener(() => gc.LoadScene(NextLevelName()));
    }

    private void OnDestroy()
    {
        if (gc != null) gc.GameEnded -= Show;
    }

    private void Show(bool won)
    {
        UITheme t = UITheme.Current;
        GameStrings s = GameStrings.Current;
        GameStats st = gc.Stats;

        titleText.text = won ? s.victoryTitle : s.defeatTitle;
        titleText.color = won ? t.success : t.danger;
        subtitleText.text = string.Format(s.wavesSurvivedFormat, st.wavesCleared, gc.TotalWaves);

        string muted = UITheme.Hex(t.textMuted);
        string Line(string label, int v) => $"<color=#{muted}>{label}</color><pos=70%>{v}";
        statsText.text =
            Line("AI models stopped", st.enemiesKilled) + "\n" +
            Line($"{s.skillPointsName} earned", st.skillPointsEarned) + "\n" +
            Line($"{s.skillPointsName} spent", st.skillPointsSpent) + "\n" +
            Line("Research completed", st.perksBought) + "\n" +
            Line("Research acquired by AI", st.perksLost);

        StringBuilder sb = new();
        foreach (FactionDefinition f in gc.Factions)
        {
            int lost = PerkManager.Instance != null ? PerkManager.Instance.LostCount(f) : 0;
            sb.Append($"<color=#{UITheme.Hex(f.color)}><b>{f.displayName}</b></color><pos=55%>{gc.GetTotalHits(f)} breaches<pos=80%>{lost} acquired\n");
        }
        factionText.text = sb.ToString();

        nextButton.gameObject.SetActive(won && NextLevelName() != null);

        group.blocksRaycasts = true;
        group.interactable = true;
        group.DOFade(1f, 0.35f).SetUpdate(true);
        if (card != null)
        {
            card.localScale = Vector3.one * 0.85f;
            card.DOScale(1f, 0.35f).SetUpdate(true);
        }
    }

    private static string NextLevelName()
    {
        string n = SceneManager.GetActiveScene().name;
        if (n.StartsWith("Level") && int.TryParse(n.Substring(5), out int i))
        {
            string next = "Level" + (i + 1);
            if (Application.CanStreamedLevelBeLoaded(next)) return next;
        }
        return null;
    }
}
