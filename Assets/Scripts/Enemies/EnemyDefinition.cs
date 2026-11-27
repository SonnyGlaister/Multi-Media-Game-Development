using UnityEngine;

// A model by an AI company.
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
    [Tooltip("0 = full knockback, 0.5 = pushed half as far, 1 = can't be knocked back.")]
    [Range(0f, 1f)] public float knockbackResistance = 0f;

    [Header("Look")]
    public float scale;
    public bool useFactionColor;
    public Color customColor;
    public GameObject modelPrefab;

    [Header("Presentation")]
    [Tooltip("Model tier: 1 = smallest model, higher = bigger/stronger.")]
    public int tier = 1;
    [Tooltip("Short label for previews, e.g. S, M, L, XL or a model-size name.")]
    public string tierLabel = "";
    public Sprite icon;
    [TextArea] public string description = "";
    [Header("Spawning")]
    public int appearWave;
    public int minGroupSize;
    public int maxGroupSize;

    public Color Tint => useFactionColor && faction != null ? faction.color : customColor;
}