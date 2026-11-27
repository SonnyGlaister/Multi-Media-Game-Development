using UnityEngine;

// Attack type (in the future for all the faction types)
[RequireComponent(typeof(TowerController))]
public abstract class TowerAttack : MonoBehaviour
{
    protected TowerController Tower { get; private set; }
    protected TowerStats Stats => Tower.Stats;

    // Optionally hides the targeting dropdown in the info panel
    public virtual bool UsesTargeting => true;

    protected virtual void Awake()
    {
        Tower = GetComponent<TowerController>();
    }

    public abstract bool TryAttack();
}