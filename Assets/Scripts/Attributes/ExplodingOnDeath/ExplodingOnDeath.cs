using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


[CreateAssetMenu(menuName = "Game/Enemy Attributes/Exploding On Death")]
// Instance of Enemy Attribute
public class ExplodingOnDeath : EnemyAttributeDefinition
{
    public int damage;
    public float radius;
    public bool friendlyFire;
    public override void ApplyTo(EnemyController enemy)
    {
        // addComponent associated to the relevant behaviour to the applied enemy.
        ExplodeOnDeathBehaviour behaviour = enemy.gameObject.AddComponent<ExplodeOnDeathBehaviour>();
        // Reassign the default values.
        behaviour.damage = damage;
        behaviour.radius = radius;
        behaviour.friendlyFire = friendlyFire;
        // When enemy reaches base, it inflicts explosion damage instead of 'reached base' damage
        enemy.Definition.damageToBase = damage;
    }
}
