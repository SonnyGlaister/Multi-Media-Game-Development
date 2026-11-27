using System;
using System.Collections.Generic;
using UnityEngine;

public enum PerkState { Available, CantAfford, NeedsPrevious, Owned, Revoked, Hidden }

public class PerkManager : MonoBehaviour
{
    public static PerkManager Instance { get; private set; }

    [Tooltip("Can a perk the AI took be bought again?")]
    public bool canRebuyRevoked = false;

    public event Action<PerkDefinition> PerkBought;
    public event Action<PerkDefinition> PerkRevoked;
    public event Action StateChanged;

    public TowerDefinition[] Branches =>
        TowerPlacer.Instance != null && TowerPlacer.Instance.towers != null ? TowerPlacer.Instance.towers : new TowerDefinition[0];

    public int LostCount => revoked.Count;
    public PerkDefinition NextToLose => owned.Count > 0 ? owned[owned.Count - 1] : null;

    private readonly List<PerkDefinition> owned = new();
    private readonly HashSet<PerkDefinition> revoked = new();

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
        gc.PressureMaxed += RevokeLatest;
    }

    public bool IsOwned(PerkDefinition perk) => owned.Contains(perk);

    public PerkState GetState(TowerDefinition tower, int index)
    {
        if (index < 0) return PerkState.Owned;
        if (index >= tower.perks.Length) return PerkState.Hidden;

        PerkDefinition perk = tower.perks[index];
        if (owned.Contains(perk)) return PerkState.Owned;
        if (revoked.Contains(perk) && !canRebuyRevoked) return PerkState.Revoked;
        if (index > 0 && !owned.Contains(tower.perks[index - 1])) return PerkState.NeedsPrevious;
        return GameController.Instance.CanAfford(perk.cost) ? PerkState.Available : PerkState.CantAfford;
    }

    public bool TryBuy(TowerDefinition tower, int index)
    {
        if (GetState(tower, index) != PerkState.Available) return false;
        PerkDefinition perk = tower.perks[index];
        if (!GameController.Instance.Purchase(perk.cost)) return false;

        owned.Add(perk);
        revoked.Remove(perk);
        OnResearchChanged(tower, perk, true);
        PerkBought?.Invoke(perk);
        return true;
    }

    private void RevokeLatest()
    {
        PerkDefinition perk = NextToLose;
        if (perk == null) return;

        owned.RemoveAt(owned.Count - 1);
        revoked.Add(perk);
        GameController.Instance.ShowMessage(string.Format(GameStrings.Current.perkTakenFormat, perk.displayName), MessageType.Danger);

        OnResearchChanged(BranchOf(perk), perk, false);
        PerkRevoked?.Invoke(perk);
    }

    private void OnResearchChanged(TowerDefinition tower, PerkDefinition perk, bool gained)
    {
        if (perk.unlocksTier > 0 && tower != null)
        {
            GameStrings s = GameStrings.Current;
            string tierName = tower.GetTier(TierOf(tower)).name;
            GameController.Instance.ShowMessage(
                string.Format(gained ? s.tierUpFormat : s.tierDownFormat, tower.displayName, tierName),
                gained ? MessageType.Success : MessageType.Danger);
        }

        foreach (TowerController t in TowerController.Towers) t.Refresh();
        StateChanged?.Invoke();
    }

    public TowerDefinition BranchOf(PerkDefinition perk)
    {
        foreach (TowerDefinition t in Branches)
            if (t != null && Array.IndexOf(t.perks, perk) >= 0) return t;
        return null;
    }

    public int TierOf(TowerDefinition tower)
    {
        int tier = 1;
        foreach (PerkDefinition p in tower.perks)
            if (p != null && owned.Contains(p)) tier = Mathf.Max(tier, p.unlocksTier);
        return Mathf.Min(tier, tower.MaxTier);
    }

    public TowerStats StatsFor(TowerDefinition tower, int tier)
    {
        TowerStats s = tower.GetTier(tier).stats.Copy();
        foreach (PerkDefinition p in tower.perks)
        {
            if (p == null || !owned.Contains(p)) continue;
            s.Add(p.bonus);
            s.cooldown *= p.cooldownMultiplier;
            s.range *= p.rangeMultiplier;
        }
        return s;
    }
}
