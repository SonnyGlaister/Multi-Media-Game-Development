using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.UI;

public class EnemyController : MonoBehaviour
{
    public int maxHealth = 3;
    public int reward = 10;
    public int damageToBase = 1;

    public string type;
    private int health;
    private CinemachinePath path;
    private CinemachineDollyCart cart;
    public Image RemainingHealth;

    public float DistanceTravelled => cart != null ? cart.m_Position : 0f;
    public bool IsAlive { get; private set; } = true;

    private GameObject HealthBar;
    private float maxHealthBarWidth;

    // Start is called before the first frame update
    void Start()
    {
        health = maxHealth;

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
            GameController.Instance.EnemyKilled(reward);
            Destroy(gameObject);
        }

        if (RemainingHealth != null && maxHealth > 0)
        {
            RemainingHealth.fillAmount = Mathf.Clamp01((float)health / maxHealth);
        }
    }

    public void ReachEnd()
    {
        IsAlive = false;
        GameController.Instance.EnemyReachedBase(damageToBase, type);
        Destroy(gameObject);
    }
}
