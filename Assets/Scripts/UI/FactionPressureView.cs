using System.Collections.Generic;
using UnityEngine;

public class FactionPressureView : MonoBehaviour
{
    public FactionPressureRow rowTemplate; // disabled child
    public Transform rowParent;

    private readonly Dictionary<FactionDefinition, FactionPressureRow> rows = new();
    private GameController gc;
    private PerkManager pm;

    private void Start()
    {
        gc = GameController.Instance;
        pm = PerkManager.Instance;

        foreach (FactionDefinition f in gc.Factions)
        {
            FactionPressureRow row = Instantiate(rowTemplate, rowParent);
            row.gameObject.SetActive(true);
            row.Bind(f);
            rows[f] = row;
        }

        gc.PressureChanged += OnPressure;
        pm.PerkRevoked += OnRevoked;
        pm.StateChanged += RefreshAll;
    }

    private void OnDestroy()
    {
        if (gc != null) gc.PressureChanged -= OnPressure;
        if (pm != null)
        {
            pm.PerkRevoked -= OnRevoked;
            pm.StateChanged -= RefreshAll;
        }
    }

    private void OnPressure(FactionDefinition f, int value)
    {
        if (rows.TryGetValue(f, out FactionPressureRow r)) r.OnPressureChanged(value);
    }

    private void OnRevoked(FactionDefinition f, int tier)
    {
        if (rows.TryGetValue(f, out FactionPressureRow r)) r.OnRevoked();
    }

    private void RefreshAll()
    {
        foreach (FactionPressureRow r in rows.Values) r.Refresh();
    }
}