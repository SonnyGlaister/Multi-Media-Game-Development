using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


[CreateAssetMenu(menuName = "Game/Enemy Attributes/Splitting")]
// Instance of Enemy Attribute
public class SplittingOnDeathDefinition : EnemyAttributeDefinition
{
    public float splitAmount;
    public override void ApplyTo(EnemyController enemy)
    {
        // addComponent associated to the relevant behaviour to the applied enemy.
        SplittingOnDeathDefinition behaviour = enemy.gameObject.AddComponent<SplittingOnDeathDefinition>();
        // Reassign the default values.
        behaviour.splitAmount = splitAmount;
    }
}
