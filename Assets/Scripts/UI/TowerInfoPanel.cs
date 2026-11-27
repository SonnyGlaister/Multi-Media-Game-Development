using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Tier, stats, targeting, sell
public class TowerInfoPanel : MonoBehaviour
{
    public RectTransform panel;
    public CanvasGroup group;
    public Image icon;
    public TextMeshProUGUI nameText;
    [Tooltip("e.g. 'Physical · Tier 2', in the faction colour.")]
    public TextMeshProUGUI tierText;
    public TextMeshProUGUI statsText;
    public TMP_Dropdown targetingDropdown;
    public Button sellButton;
    public TextMeshProUGUI sellLabel;
    public Button closeButton;
    [Tooltip("Gap between the tower and the panel, in reference pixels.")]
    public float screenGap = 70f;
    public float sellConfirmSeconds = 2f;

    private TowerController tower;
    private Camera cam;
    private float confirmUntil;

    private void Start()
    {
        cam = Camera.main;

        targetingDropdown.ClearOptions();
        targetingDropdown.AddOptions(new List<string>(Enum.GetNames(typeof(TargetMode))));
        targetingDropdown.onValueChanged.AddListener(i =>
        {
            if (tower != null) tower.targetMode = (TargetMode)i;
        });

        sellButton.onClick.AddListener(OnSellClicked);
        closeButton.onClick.AddListener(() => TowerSelection.Instance.Deselect());
        SetVisible(false);
    }

    private void LateUpdate()
    {
        TowerController current = TowerSelection.Instance != null ? TowerSelection.Instance.Selected : null;
        if (current != tower) Bind(current);

        if (tower == null)
        {
            SetVisible(false);
            return;
        }
        Refresh();
        Position();
    }

    private void Bind(TowerController t)
    {
        tower = t;
        confirmUntil = 0f;
        targetingDropdown.Hide();
        SetVisible(t != null);
        if (t == null) return;

        targetingDropdown.SetValueWithoutNotify((int)t.targetMode);
        panel.DOKill(true);
        panel.DOPunchScale(Vector3.one * 0.06f, 0.18f, 1, 0f).SetUpdate(true);
    }

    private void Refresh()
    {
        GameStrings s = GameStrings.Current;
        TowerTier data = tower.TierData;

        icon.sprite = data.icon;
        icon.enabled = data.icon != null;
        nameText.text = data.name;
        if (tierText != null)
        {
            tierText.text = $"{tower.Definition.displayName}  ·  {string.Format(s.tierFormat, tower.Tier)}";
            tierText.color = tower.Definition.color;
        }
        statsText.text = tower.StatsText();

        bool usesTargeting = tower.Attack == null || tower.Attack.UsesTargeting;
        if (targetingDropdown.gameObject.activeSelf != usesTargeting)
            targetingDropdown.gameObject.SetActive(usesTargeting);

        bool confirming = Time.unscaledTime < confirmUntil;
        sellLabel.text = string.Format(confirming ? s.sellConfirmFormat : s.sellFormat, tower.SellValue);
    }

    private void OnSellClicked()
    {
        if (tower == null) return;
        if (Time.unscaledTime < confirmUntil) tower.Sell();
        else confirmUntil = Time.unscaledTime + sellConfirmSeconds;
    }

    private void Position()
    {
        Vector3 screen = cam.WorldToScreenPoint(tower.transform.position);
        if (screen.z < 0f) return;

        float scale = panel.lossyScale.x;
        Vector2 size = panel.rect.size * scale;
        float gap = screenGap * scale;

        float x = screen.x + gap;
        if (x + size.x > Screen.width) x = screen.x - gap - size.x;
        float y = Mathf.Clamp(screen.y, size.y / 2f, Screen.height - size.y / 2f);
        panel.position = new Vector2(x, y);
    }

    private void SetVisible(bool v)
    {
        group.alpha = v ? 1f : 0f;
        group.blocksRaycasts = v;
        group.interactable = v;
    }
}
