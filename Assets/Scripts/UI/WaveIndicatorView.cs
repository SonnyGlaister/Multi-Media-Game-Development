using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WaveIndicatorView : MonoBehaviour
{
    public RectTransform pill;
    public Image pillBackground;
    public TextMeshProUGUI waveText;
    public Image progressFill;
    public WaveBanner banner;

    private GameController gc;
    private Color normalBackground;

    private void Start()
    {
        gc = GameController.Instance;
        normalBackground = pillBackground.color;
        gc.WaveStarted += OnWaveStarted;
        gc.SetupStarted += OnSetupStarted;
        Refresh();
    }

    private void OnDestroy()
    {
        if (gc == null) return;
        gc.WaveStarted -= OnWaveStarted;
        gc.SetupStarted -= OnSetupStarted;
    }

    private void Update()
    {
        EnemySpawner sp = gc.spawner;
        int total = sp.TotalThisWave;
        if (gc.HasWaveStarted)
        {
            int defeated = Mathf.Max(0, sp.SpawnedThisWave - EnemyController.Alive.Count);
            progressFill.fillAmount = total > 0 ? (float)defeated / total : 0f;
        }
        else
        {
            progressFill.fillAmount = 0f;
        }
    }

    private void OnSetupStarted(int wave) => Refresh();

    private void OnWaveStarted(int wave)
    {
        Refresh();
        GameStrings s = GameStrings.Current;
        if (banner != null) banner.Play(gc.IsFinalWave ? s.finalWave : string.Format(s.waveBannerFormat, wave + 1), pill);
    }

    private void Refresh()
    {
        GameStrings s = GameStrings.Current;
        int n = Mathf.Min(gc.currentWave + 1, gc.TotalWaves);
        waveText.text = gc.IsFinalWave && gc.TotalWaves > 1 ? s.finalWave : string.Format(s.waveFormat, n, gc.TotalWaves);
        pillBackground.color = gc.IsFinalWave && gc.TotalWaves > 1
            ? Color.Lerp(normalBackground, UITheme.Current.danger, 0.55f)
            : normalBackground;
    }
}
