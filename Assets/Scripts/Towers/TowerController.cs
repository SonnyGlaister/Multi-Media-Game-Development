using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public enum TargetMode { First, Last, Strongest, Closest }

public class TowerController : MonoBehaviour
{
    public static readonly List<TowerController> Towers = new();

    [Header("Parts")]
    [Tooltip("Empty child the current tier's model is spawned under.")]
    public Transform modelSlot;
    [Tooltip("Child SpriteRenderer lying flat on the ground, 1 unit wide at scale 1, starts disabled.")]
    public SpriteRenderer rangeRing;

    [Header("Targeting")]
    public TargetMode targetMode = TargetMode.First;

    public TowerDefinition Definition { get; private set; }
    public int Tier { get; private set; }
    public TowerTier TierData => Definition.GetTier(Tier);
    public TowerStats Stats { get; private set; } = new TowerStats();
    public string DisplayName => TierData.name;
    public TowerAttack Attack { get; private set; }

    public int PaidCost { get; set; }
    public int SellValue => Mathf.RoundToInt(PaidCost * Definition.sellRefund);

    private float timer;

    private void Awake() => Attack = GetComponent<TowerAttack>();
    private void OnEnable() => Towers.Add(this);
    private void OnDisable() => Towers.Remove(this);

    private void OnDestroy()
    {
        if (TowerSelection.Instance != null && TowerSelection.Instance.Selected == this)
            TowerSelection.Instance.Deselect();
    }

    public void Init(TowerDefinition definition)
    {
        Definition = definition;
        name = definition.displayName;
        Refresh();
    }

    public void Refresh()
    {
        if (Definition == null) return;

        PerkManager pm = PerkManager.Instance;
        int tier = pm != null ? pm.TierOf(Definition) : 1;
        Stats = pm != null ? pm.StatsFor(Definition, tier) : Definition.GetTier(tier).stats.Copy();

        if (tier != Tier)
        {
            bool firstTime = Tier == 0;
            Tier = tier;
            SwapModel();
            if (!firstTime)
            {
                modelSlot.DOKill(true);
                modelSlot.DOPunchScale(Vector3.one * 0.3f, 0.4f, 1, 0f);
            }
        }

        if (rangeRing != null && rangeRing.enabled) ShowRange(rangeRing.color); // range may have changed
    }

    private void Update()
    {
        if (Definition == null || Attack == null) return;
        timer += Time.deltaTime;
        if (timer >= Stats.cooldown && Attack.TryAttack()) timer = 0f;
    }

    private void SwapModel()
    {
        foreach (Transform child in modelSlot) Destroy(child.gameObject);
        if (TierData.model != null) Instantiate(TierData.model, modelSlot);
    }

    // Targeting
    public EnemyController FindTarget(Predicate<EnemyController> filter = null)
    {
        List<EnemyController> list = FindTargets(1, filter);
        return list.Count > 0 ? list[0] : null;
    }

    public List<EnemyController> FindTargets(int count, Predicate<EnemyController> filter = null)
    {
        List<EnemyController> inRange = EnemyController.InRadius(transform.position, Stats.range);
        if (filter != null) inRange.RemoveAll(e => !filter(e));
        inRange.Sort((a, b) => Score(b).CompareTo(Score(a))); // highest score first
        if (inRange.Count > count) inRange.RemoveRange(count, inRange.Count - count);
        return inRange;
    }

    // Higher = better target
    private float Score(EnemyController enemy) => targetMode switch
    {
        TargetMode.First => -enemy.GetDistanceAlongPath(),
        TargetMode.Last => enemy.GetDistanceAlongPath(),
        TargetMode.Strongest => enemy.Health,
        _ => -Vector3.Distance(transform.position, enemy.transform.position),
    };

    // Selling
    public void Sell()
    {
        GameController gc = GameController.Instance;
        if (gc == null || gc.IsGameOver) return;
        gc.Refund(SellValue);
        if (TowerPlacer.Instance != null) TowerPlacer.Instance.OnTowerSold(Definition);
        Destroy(gameObject);
    }

    // Range
    public void ShowRange(Color color)
    {
        if (rangeRing == null) return;
        float diameter = Stats.range * 2f;
        rangeRing.transform.localScale = Vector3.one * (diameter / transform.lossyScale.x);
        rangeRing.color = color;
        rangeRing.enabled = true;
    }

    public void HideRange()
    {
        if (rangeRing != null) rangeRing.enabled = false;
    }

    public string StatsText() => TowerStats.Describe(TierData.stats, Stats);
}
