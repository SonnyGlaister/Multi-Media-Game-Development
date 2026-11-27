using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class SplittingOnDeathBehaviour : MonoBehaviour
{
    public int splitAmount = 1;
    public EnemyDefinition childEnemy;
    public float delayBetween = 0.2f;
    private EnemyController enemy;
    public bool split = false;

    private void Awake()
    {
        enemy = GetComponent<EnemyController>();
    }
    private void OnEnable()
    {
        if (enemy != null)
        {
            enemy.Died += this.OnDeath;
        }

    }

    private void OnDisable()
    {
        if (enemy != null)
        {
            enemy.Died -= this.OnDeath;
        }
    }

    private void OnDeath(EnemyController enemy)
    {
        if (split)
        {
            return;
        }

        split = true;
        // Require a method outside of this behaviour as the enemy is destroyed too quickly on death for the children to spawn with input delay.
        GameController.Instance.SpawnSplitChildren(enemy, childEnemy, splitAmount, delayBetween);
    }

}
