using System.Collections.Generic;
using UnityEngine;

public enum PerkState { Available, CantAfford, NeedsPrevious, Owned, Revoked, LockedOut }

public class PerkManager : MonoBehaviour
{
    public static PerkManager Instance { get; private set; }

    // Combined effect of every owned perk
    public float CooldownMultiplier { get; private set; } = 1f;
    public float RangeMultiplier { get; private set; } = 1f;
    public int DamageBonus { get; private set; }
    public int PiercingBonus { get; private set; }
    public float AOERangeBonus { get; private set; }
    public float AOEDamageMultiplier { get; private set; }

    // Per faction: how many tiers you own and how many tiers can still be bought
    private readonly Dictionary<FactionDefinition, int> owned = new();
    private readonly Dictionary<FactionDefinition, int> cap = new();

    void Awake()
    {
        Instance = this;
    }

    int Owned(FactionDefinition f) => owned.TryGetValue(f, out int v) ? v : 0;
    int Cap(FactionDefinition f) => cap.TryGetValue(f, out int v) ? v : f.perks.Length;

    void Update()
    {
        foreach (FactionDefinition f in GameController.Instance.Factions)
        {
            if (GameController.Instance.IsLockedOut(f) && Owned(f) > 0)
            {
                Revoke(f);
            }
        }
    }

    public PerkState GetState(FactionDefinition f, int tier)
    {
        if (GameController.Instance.IsLockedOut(f)) return PerkState.LockedOut;
        if (tier >= Cap(f)) return PerkState.Revoked;
        if (tier < Owned(f)) return PerkState.Owned;
        if (tier > Owned(f)) return PerkState.NeedsPrevious;
        return GameController.Instance.CanAfford(f.perks[tier].cost) ? PerkState.Available : PerkState.CantAfford;
    }

    public void TryBuy(FactionDefinition f, int tier)
    {
        if (GetState(f, tier) != PerkState.Available) return;

        GameController.Instance.Currency -= f.perks[tier].cost;
        owned[f] = tier + 1;
        Recalculate();
    }

    // Lose the highest owned perk; it can't be bought again.
    void Revoke(FactionDefinition f)
    {
        int remaining = Owned(f) - 1;
        owned[f] = remaining;
        cap[f] = remaining;

        if (remaining > 0)
        {
            GameController.Instance.ReducePressure(f, f.lockoutThreshold);
        }
        Recalculate();
    }

    void Recalculate()
    {
        CooldownMultiplier = 1f;
        RangeMultiplier = 1f;
        DamageBonus = 0;
        PiercingBonus = 0;
        AOERangeBonus = 0;

        foreach (FactionDefinition f in GameController.Instance.Factions)
        {
            for (int i = 0; i < Owned(f); i++)
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