using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Wave")]
public class WaveDefinition : ScriptableObject
{
    public int startTime;
    public float speedMultiplier;
    public EnemySpawnBlockDefinition[] enemies;

}
