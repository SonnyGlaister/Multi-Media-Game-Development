using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Enemy")]
public class EnemyDefinition : ScriptableObject
{
    public string displayName;
    public FactionDefinition faction;

    [Header("Stats")]
    public int maxHealth;
    public float speed;
    public int reward;
    public int damageToBase;
    public int pressurePerHit;

    [Header("Look")]
    public float scale;
    public bool useFactionColor;
    public Color customColor;
    public GameObject modelPrefab;

    public Color Tint => useFactionColor && faction != null ? faction.color : customColor;
}
