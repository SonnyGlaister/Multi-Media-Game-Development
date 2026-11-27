using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TowerDrawer : MonoBehaviour
{
    public RectTransform drawer;
    [Tooltip("Y position when collapsed (negative = slides down; the toggle tab stays visible).")]
    public float collapsedY = -180f;
    public Button toggleButton;
    public TextMeshProUGUI toggleLabel;
    public KeyCode toggleKey = KeyCode.B;
    public Transform cardParent;
    public TowerCard cardTemplate;

    private readonly List<TowerCard> cards = new();
    private float expandedY;
    private bool expanded = true;

    private void Start()
    {
        expandedY = drawer.anchoredPosition.y;

        foreach (TowerDefinition tower in TowerPlacer.Instance.towers)
        {
            if (tower == null) continue;
            TowerCard card = Instantiate(cardTemplate, cardParent);
            card.gameObject.SetActive(true);
            card.Setup(tower, cards.Count + 1);
            cards.Add(card);
        }

        toggleButton.onClick.AddListener(Toggle);
        UpdateToggleLabel();
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey)) Toggle();

        for (int i = 0; i < cards.Count && i < 9; i++)
            if (Input.GetKeyDown(KeyCode.Alpha1 + i)) cards[i].StartClickPlacement();
    }

    public void Toggle()
    {
        expanded = !expanded;
        drawer.DOKill();
        drawer.DOAnchorPosY(expanded ? expandedY : collapsedY, 0.2f).SetUpdate(true);
        UpdateToggleLabel();
    }

    private void UpdateToggleLabel()
    {
        GameStrings s = GameStrings.Current;
        toggleLabel.text = expanded ? s.hideTowersButton : s.buildTowersButton;
    }
}
