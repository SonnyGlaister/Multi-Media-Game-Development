using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class SkillTreeMenu : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject SkillTree;
    public static SkillTreeMenu Instance;
    public bool isOpen = false;
    void Start()
    {
        SkillTree.SetActive(false);
    }
    private void Awake()
    {
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (!GameController.Instance.HasGameStarted) { return; }
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (isOpen)
            {
                SkillTree.SetActive(false);
                isOpen = false;
            }
            else
            {
                SkillTree.SetActive(true);
                isOpen = true;
            }
            return;
        }
    }
}