using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LevelSelect : MonoBehaviour
{
    public Button Level1Button;
    public Button Level2Button;
    public Button Level3Button;

    // Start is called before the first frame update
    void Start()
    {
        Level1Button.onClick.AddListener(() => StartLevel(1));
        Level2Button.onClick.AddListener(() => StartLevel(2));
        Level3Button.onClick.AddListener(() => StartLevel(3));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void StartLevel(int level)
    {
        SceneManager.LoadScene("Level"+level, LoadSceneMode.Single);
    }
}
