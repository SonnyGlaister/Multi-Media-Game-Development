using System.Collections.Generic;
using UnityEngine;

public enum TargetMode { First, Last, Strongest, Closest }

public class TowerController : MonoBehaviour
{
    public static readonly List<TowerController> Towers = new();

    [Header("Info (build drawer + info panel)")]
    public string displayName = "Tower";
    public Sprite icon;
    [TextArea] public string description = "";

    [Header("Base stats")]
    public float range = 8;
    public int damage = 1;
    public int piercing = 0;
    public float fireCooldown = 1f;
    public float aoeRange = 0;
    public float aoeDamage = 0;

    [Header("Targeting")]
    public TargetMode targetMode = TargetMode.First;

    [Header("Projectile")]
    public Rigidbody projectile;
    public float projectileSpeed = 20;

    [Header("Economy")]
    public int startCost = 5;
    [Tooltip("How many of this tower cost Start Cost before the price grows.")]
    public int flatPriceCount = 2;
    [Tooltip("Each tower after that costs this many times more.")]
    public float costGrowth = 2f;
    [Range(0f, 1f)] public float sellRefund = 0.7f;

    [Header("Range ring")]
    [Tooltip("Child SpriteRenderer lying flat on the ground, 1 unit wide at scale 1, starts disabled.")]
    public SpriteRenderer rangeRing;

    // Effective stats
    private static PerkManager P => PerkManager.Instance;
    public float Cooldown => fireCooldown * (P != null ? P.CooldownMultiplier : 1f);
    public float Range => range * (P != null ? P.RangeMultiplier : 1f);
    public int Damage => damage + (P != null ? P.DamageBonus : 0);
    public int Piercing => piercing + (P != null ? P.PiercingBonus : 0);
    public float AOERange => aoeRange + (P != null ? P.AOERangeBonus : 0f);
    public float AOEDamage => aoeDamage + (P != null ? P.AOEDamageMultiplier : 0f);
    public float ShotsPerSecond => Cooldown > 0f ? 1f / Cooldown : 0f;
    public float BaseShotsPerSecond => fireCooldown > 0f ? 1f / fireCooldown : 0f;
    public float DamagePerSecond => Damage * ShotsPerSecond;

    // This tower
    public TowerController SourcePrefab { get; set; }
    public int PaidCost { get; set; }
    public int SellValue => Mathf.RoundToInt(PaidCost * sellRefund);

    private float timer;

    private void OnEnable() => Towers.Add(this);
    private void OnDisable() => Towers.Remove(this);

    private void OnDestroy()
    {
        if (TowerSelection.Instance != null && TowerSelection.Instance.Selected == this)
            TowerSelection.Instance.Deselect();
    }

    private void Update()
    {
        timer += Time.deltaTime;
        EnemyController target = FindTarget();
        if (target != null && timer >= Cooldown)
        {
            Shoot(target);
            timer = 0f;
        }
    }

    // Price of next tower
    public int CostFor(int alreadyPlaced)
    {
        if (alreadyPlaced < flatPriceCount) return startCost;
        return Mathf.RoundToInt(startCost * Mathf.Pow(costGrowth, alreadyPlaced - flatPriceCount + 1));
    }

    public void Sell()
    {
        GameController gc = GameController.Instance;
        if (gc == null || gc.IsGameOver) return;
        gc.Refund(SellValue);
        if (TowerPlacer.Instance != null) TowerPlacer.Instance.OnTowerSold(SourcePrefab);
        Destroy(gameObject);
    }

    public void ShowRange(Color color)
    {
        if (rangeRing == null) return;
        float diameter = Range * 2f;
        rangeRing.transform.localScale = Vector3.one * (diameter / transform.lossyScale.x);
        rangeRing.color = color;
        rangeRing.enabled = true;
    }

    public void HideRange()
    {
        if (rangeRing != null) rangeRing.enabled = false;
    }

    public string StatsText()
    {
        string bonus = UITheme.Hex(UITheme.Current.bonus);
        string Line(string label, float baseValue, float value, string format)
        {
            string v = Mathf.Approximately(baseValue, value)
                ? value.ToString(format)
                : $"{baseValue.ToString(format)} → <color=#{bonus}>{value.ToString(format)}</color>";
            return $"{label}: {v}\n";
        }

        string text = Line("Damage", damage, Damage, "0")
                    + Line("Shots/sec", BaseShotsPerSecond, ShotsPerSecond, "0.##")
                    + Line("Range", range, Range, "0.#");
        if (Piercing > 0) text += Line("Pierce", piercing, Piercing, "0");
        if (AOERange > 0f) text += Line("Splash", aoeRange, AOERange, "0.#");
        return text.TrimEnd('\n');
    }

    private EnemyController FindTarget()
    {
        EnemyController best = null;
        float bestScore = float.NegativeInfinity;
        float r = Range;

        foreach (EnemyController enemy in EnemyController.Alive)
        {
            if (!enemy.IsAlive) continue;
            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist > r) continue;

            // Higher score = better target
            float score = targetMode switch
            {
                TargetMode.First => -enemy.GetDistanceAlongPath(), // closest to HQ
                TargetMode.Last => enemy.GetDistanceAlongPath(),
                TargetMode.Strongest => enemy.Health,
                _ => -dist, // Closest
            };

            if (score > bestScore)
            {
                bestScore = score;
                best = enemy;
            }
        }
        return best;
    }

    private void Shoot(EnemyController target)
    {
        Rigidbody instance = Instantiate(projectile, transform.position, Quaternion.identity);
        instance.GetComponent<ProjectileController>()
                .Launch(target.transform, Damage, projectileSpeed, Piercing, AOERange, AOEDamage);
    }
}