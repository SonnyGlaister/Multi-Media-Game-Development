using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/SpawnBlock")]
public class EnemySpawnBlockDefinition : ScriptableObject
{
    [Header("Create")]
    public EnemyDefinition enemyType;
    public int enemyAmount;

    [Header("Delays")]
    public float delayBetween;
    public float delayAfterGroup;
}
