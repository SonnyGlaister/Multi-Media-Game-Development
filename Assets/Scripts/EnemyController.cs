using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.UI;

public class EnemyController : MonoBehaviour
{
    public Image RemainingHealth;
    public Renderer bodyRenderer;

    public EnemyDefinition Definition { get; private set; }
    public FactionDefinition Faction => Definition != null ? Definition.faction : null;

    public float DistanceTravelled => cart != null ? cart.m_Position : 0f;
    public bool IsAlive { get; private set; } = true;

    private int health;
    private CinemachinePath path;
    private CinemachineDollyCart cart;


    // Called by the spawner right after Instantiate
    public void Init(EnemyDefinition definition)
    {
        Definition = definition;
        name = definition.displayName;
        health = definition.maxHealth;
        transform.localScale = Vector3.one * definition.scale;

        cart = GetComponent<CinemachineDollyCart>();
        if (cart != null) cart.m_Speed = definition.speed;

        if (definition.modelPrefab != null)
        {
            bodyRenderer.enabled = false;
            Instantiate(definition.modelPrefab, transform);
        }
        else if (bodyRenderer != null)
        {
            bodyRenderer.material.color = definition.Tint;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        path = GameObject.Find("LevelPath").GetComponent<CinemachinePath>();
        cart = GetComponent<CinemachineDollyCart>();
        if (cart != null && path != null)
        {
            cart.m_Path = path;
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (!IsAlive || cart == null || path == null) return;

        if (cart.m_Position >= path.PathLength)
        {
            ReachEnd();
        }
        
    }

    public void TakeDamage(int amount)
    {
        if (!IsAlive) return;

        health -= amount;
        if (health <= 0)
        {
            IsAlive = false;
            GameController.Instance.EnemyKilled(Definition.reward);
            Destroy(gameObject);
        }

        if (RemainingHealth != null && Definition.maxHealth > 0)
        {
            RemainingHealth.fillAmount = Mathf.Clamp01((float)health / Definition.maxHealth);
        }
    }

    public void ReachEnd()
    {
        IsAlive = false;
        GameController.Instance.EnemyReachedBase(Definition);
        Destroy(gameObject);
    }
}
