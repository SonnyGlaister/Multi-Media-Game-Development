using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


[CreateAssetMenu(menuName = "Game/Enemy Attributes/Flying")]
// Instance of Enemy Attribute
public class FlyingAttributeDefinition : EnemyAttributeDefinition
{
    public float height;
    public override void ApplyTo(EnemyController enemy)
    {
        // addComponent associated to the relevant behaviour to the applied enemy.
        FlyingAttributeBehaviour behaviour = enemy.gameObject.AddComponent<FlyingAttributeBehaviour>();
        // Reassign the default values.
        behaviour.height = height;
    }
}
