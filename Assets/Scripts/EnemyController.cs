using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class EnemyController : MonoBehaviour
{
    private CinemachinePath path;
    private CinemachineDollyCart cart;
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
        
    }
}
