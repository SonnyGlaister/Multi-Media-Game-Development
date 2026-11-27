using System.Collections.Generic;
using UnityEngine;

// Projectile attack (Physical faction)
public class ProjectileAttack : TowerAttack
{
    [Tooltip("Optional point projectiles start from. Empty = the tower's position.")]
    public Transform muzzle;

    public override bool TryAttack()
    {
        GameObject prefab = Tower.TierData.spawnPrefab;
        if (prefab == null) return false;

        List<EnemyController> targets = Tower.FindTargets(Mathf.Max(1, Stats.projectiles));
        if (targets.Count == 0) return false;

        Vector3 from = muzzle != null ? muzzle.position : transform.position;
        foreach (EnemyController target in targets)
        {
            ProjectileController p = Instantiate(prefab, from, Quaternion.identity).GetComponent<ProjectileController>();
            p.Launch(target.transform, Stats.damage, Stats.projectileSpeed, Stats.piercing, Stats.aoeRadius);
        }
        return true;
    }
}
