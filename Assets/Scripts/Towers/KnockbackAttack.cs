using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

// Mechanic (knocks enemies in range back down the route)
public class KnockbackAttack : TowerAttack
{
    [Header("Spin")]
    [Tooltip("Spin the model once per hit (the bat / fan).")]
    public bool spinModel = true;
    public float spinTime = 0.3f;

    [Header("Shockwave (optional)")]
    [Tooltip("Flat circle sprite, 1 unit wide (NoBuildDisc works). Pulses out to the range on each hit.")]
    public SpriteRenderer pulsePrefab;
    public float pulseTime = 0.25f;
    [Range(0f, 1f)] public float pulseAlpha = 0.45f;

    public override bool UsesTargeting => false;
    public override bool TryAttack()
    {
        List<EnemyController> hits = EnemyController.InRadius(transform.position, Stats.range);
        if (hits.Count == 0) return false;

        foreach (EnemyController e in hits)
        {
            e.TakeDamage(Stats.damage);
            e.KnockBack(Stats.knockback);
        }

        Spin();
        Pulse();
        return true;
    }

    private void Spin()
    {
        if (!spinModel || Tower.modelSlot == null) return;
        Tower.modelSlot.DOKill(true);
        Tower.modelSlot.DOLocalRotate(new Vector3(0f, 360f, 0f), spinTime, RotateMode.LocalAxisAdd)
            .SetEase(Ease.OutQuad)
            .SetLink(gameObject);
    }

    private void Pulse()
    {
        if (pulsePrefab == null) return;

        SpriteRenderer pulse = Instantiate(pulsePrefab, transform.position + Vector3.up * 0.06f, pulsePrefab.transform.rotation);
        Color c = Tower.Definition.color;
        c.a = pulseAlpha;
        pulse.color = c;
        pulse.transform.localScale = Vector3.zero;

        DOTween.Sequence()
            .Append(pulse.transform.DOScale(Stats.range * 2f, pulseTime).SetEase(Ease.OutQuad))
            .Join(pulse.DOFade(0f, pulseTime).SetEase(Ease.InQuad))
            .OnComplete(() => Destroy(pulse.gameObject))
            .SetLink(pulse.gameObject);
    }
}
