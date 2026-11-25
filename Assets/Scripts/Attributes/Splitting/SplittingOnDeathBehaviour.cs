using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class SplittingOnDeathBehaviour : MonoBehaviour
{
    public int splitAmount = 1;

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

        // For loop, create x amount of extra enemies with unique statistics to original one
        // Inherit position from original.

    }
}
