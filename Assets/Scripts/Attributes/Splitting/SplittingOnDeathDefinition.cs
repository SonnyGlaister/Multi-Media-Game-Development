using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


[CreateAssetMenu(menuName = "Game/Enemy Attributes/Splitting")]
// Instance of Enemy Attribute
public class SplittingOnDeathDefinition : EnemyAttributeDefinition
{
    public int splitAmount;
    public EnemyDefinition childEnemy;
    public float delayBetween;
    public override void ApplyTo(EnemyController enemy)
    {
        // addComponent associated to the relevant behaviour to the applied enemy.
        SplittingOnDeathBehaviour behaviour = enemy.gameObject.AddComponent<SplittingOnDeathBehaviour>();
        // Reassign the default values.
        behaviour.splitAmount = splitAmount;
        behaviour.childEnemy = childEnemy;
        behaviour.delayBetween = delayBetween; 
    }
}
