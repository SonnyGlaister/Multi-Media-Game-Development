using System;
using System.Collections.Generic;
using UnityEngine;

public enum PerkState { Available, CantAfford, NeedsPrevious, Owned, Revoked, LockedOut }

public class PerkManager : MonoBehaviour
{
    public static PerkManager Instance { get; private set; }

    public event Action<FactionDefinition, int> PerkRevoked; // faction, tier lost
    public event Action StateChanged;

    // Combined effect of every owned perk
    public float CooldownMultiplier { get; private set; } = 1f;
    public float RangeMultiplier { get; private set; } = 1f;
    public int DamageBonus { get; private set; }
    public int PiercingBonus { get; private set; }
    public float AOERangeBonus { get; private set; }
    public float AOEDamageMultiplier { get; private set; }

    private readonly Dictionary<FactionDefinition, int> owned = new();
    private readonly Dictionary<FactionDefinition, int> cap = new();
    private readonly Dictionary<FactionDefinition, int> lost = new();
    private readonly HashSet<FactionDefinition> announcedCapture = new();

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Start()
    {
        GameController gc = GameController.Instance;
        gc.SkillPointsChanged += _ => StateChanged?.Invoke();
        gc.PressureChanged += (f, p) => StateChanged?.Invoke();
    }

    public int OwnedCount(FactionDefinition f) => owned.TryGetValue(f, out int v) ? v : 0;
    public int LostCount(FactionDefinition f) => lost.TryGetValue(f, out int v) ? v : 0;
    private int Cap(FactionDefinition f) => cap.TryGetValue(f, out int v) ? v : f.perks.Length;

    public PerkDefinition NextToLose(FactionDefinition f)
    {
        int o = OwnedCount(f);
        return o > 0 ? f.perks[o - 1] : null;
    }

    private void Update()
    {
        GameController gc = GameController.Instance;
        foreach (FactionDefinition f in gc.Factions)
        {
            if (!gc.IsLockedOut(f)) continue;

            if (OwnedCount(f) > 0)
            {
                Revoke(f);
            }
            else if (announcedCapture.Add(f))
            {
                gc.ShowMessage(string.Format(GameStrings.Current.lockedToastFormat, f.displayName), MessageType.Danger);
                StateChanged?.Invoke();
            }
        }
    }

    public PerkState GetState(FactionDefinition f, int tier)
    {
        if (GameController.Instance.IsLockedOut(f)) return PerkState.LockedOut;
        if (tier >= Cap(f)) return PerkState.Revoked;
        if (tier < OwnedCount(f)) return PerkState.Owned;
        if (tier > OwnedCount(f)) return PerkState.NeedsPrevious;
        return GameController.Instance.CanAfford(f.perks[tier].cost) ? PerkState.Available : PerkState.CantAfford;
    }

    public bool TryBuy(FactionDefinition f, int tier)
    {
        if (GetState(f, tier) != PerkState.Available) return false;
        if (!GameController.Instance.Purchase(f.perks[tier].cost)) return false;

        owned[f] = tier + 1;
        GameController.Instance.Stats.perksBought++;
        Recalculate();
        StateChanged?.Invoke();
        return true;
    }

    // Lose the highest owned perk; it can't be bought again.
    private void Revoke(FactionDefinition f)
    {
        int remaining = OwnedCount(f) - 1;
        PerkDefinition lostPerk = f.perks[remaining];

        owned[f] = remaining;
        cap[f] = remaining;
        lost[f] = LostCount(f) + 1;
        GameController.Instance.Stats.perksLost++;

        if (remaining > 0)
        {
            GameController.Instance.ReducePressure(f, f.lockoutThreshold);
        }

        Recalculate();
        GameController.Instance.ShowMessage(
            string.Format(GameStrings.Current.revokedToastFormat, f.displayName, lostPerk.displayName), MessageType.Danger);
        PerkRevoked?.Invoke(f, remaining);
        StateChanged?.Invoke();
    }

    private void Recalculate()
    {
        CooldownMultiplier = 1f;
        RangeMultiplier = 1f;
        DamageBonus = 0;
        PiercingBonus = 0;
        AOERangeBonus = 0;
        AOEDamageMultiplier = 0;

        foreach (FactionDefinition f in GameController.Instance.Factions)
        {
            for (int i = 0; i < OwnedCount(f); i++)
            {
                PerkDefinition p = f.perks[i];
                CooldownMultiplier *= p.cooldownMultiplier;
                RangeMultiplier *= p.rangeMultiplier;
                DamageBonus += p.damageBonus;
                PiercingBonus += p.piercingBonus;
                AOERangeBonus += p.aoeRangeBonus;
                AOEDamageMultiplier += p.aoeDamageMultiplier;
            }
        }
    }
}