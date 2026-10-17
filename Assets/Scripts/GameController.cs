using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; }

    public int startingCurrency = 5;
    public int startingBaseHealth = 3;
    public int totalWaves = 2;

    public EnemySpawner spawner;
    public TextMeshProUGUI hudText;
    public TextMeshProUGUI messageText;
    public Button startButton;
    public GameObject skillTree;

    public int Currency { get; set; }
    public int BaseHealth { get; private set; }
    public bool IsGameOver { get; private set; }
    public bool HasGameStarted { get; private set; }

    public bool HasWaveStarted { get; private set; }
    public int redDamage { get; private set; }
    public int yellowDamage { get; private set; }

    public int currentWave { get; private set; }

    private float messageTimer;


    // Awake is called immediately to initiate the Instance
    private void Awake()
    {
        Instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        Currency = startingCurrency;
        BaseHealth = startingBaseHealth;

        HasGameStarted = false;
        IsGameOver = false;
        HasWaveStarted = true;

        redDamage = 0;
        yellowDamage = 0;

        currentWave = 0;

        if (messageText != null) messageText.text = "";
        
        startButton.onClick.AddListener(StartGame);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateHud();

        if (IsGameOver)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                ResetPerks();
            }
            return;
        }

        if (! HasWaveStarted)
        {
            return;
        }

        CheckWaveEnd();

        if (messageTimer > 0)
        {
            messageTimer -= Time.deltaTime;
            if (messageTimer <= 0 && messageText != null) messageText.text = "";
        }
    }

    // Updates currency on enemy death
    public void EnemyKilled(int reward)
    {
        if (IsGameOver) return;
        Currency += reward;
    }

    // Updates base health on enemy hit; ends game if base destroyed
    public void EnemyReachedBase(int damage, string type)
    {
        if (IsGameOver) return;
        BaseHealth = Mathf.Max(BaseHealth - damage, 0);

        if (type == "red")
        {
            redDamage ++;
        }
        if (type == "yellow")
        {
            yellowDamage ++;
        }

        if (BaseHealth <= 0)
        {
            EndGame(false);
        }
    }

    // Returns true and decreases currency if player can afford item, false otherwise
    public bool Purchase(int cost)
    {
        if (IsGameOver || !CanAfford(cost)) return false;
        Currency -= cost;
        return true;
    }

    public bool CanAfford(int cost)
    {
        if (Currency < cost)
        {
            return false;
        }
        return true;
    }

    // Show message on the screen
    public void ShowMessage(string message, float duration = 2)
    {
        if (messageText == null) return;
        messageText.text = message;
        messageTimer = duration;
    }

    // Check if wave is over, move to next wave
    private void CheckWaveEnd()
    {
        if (spawner == null || !spawner.FinishedSpawning) return;
        if (GameObject.FindGameObjectsWithTag("Enemy").Length == 0)
        {
            currentWave++;
            CheckWin();
        }
    }
    // Check if the player won the game
    private void CheckWin()
    {
        if (currentWave >= totalWaves)
        {
            EndGame(true);
        }
        else
        {
            SetupWave();
        }
    }

    private void StartGame()
    {
        HasGameStarted = true;
    }

    private void SetupWave()
    {
        HasWaveStarted = false;
        spawner.ResetSpawner();
    }

    public void StartWave()
    {
        HasWaveStarted = true;
    }

    // End the game with player winning or losing
    private void EndGame(bool won)
    {
        IsGameOver = true;
        Time.timeScale = 0;
        if (messageText != null)
        {
            if (won)
            {
                messageText.text = "YOU WIN!\nPress R to restart";
            } else
            {
                messageText.text = "BASE DESTROYED\nPress R to restart";
            }
        }
        messageTimer = 0;
    }

    // Updates hud text
    private void UpdateHud()
    {
        if (hudText == null) return;
        string hud = $"Gold: {Currency}\nBase HP: {BaseHealth}\nRed Damage: {redDamage}\nYellow Damage: {yellowDamage}\nWave Number: {currentWave+1}";
        if (spawner != null && spawner.SetupTimeRemaining > 0)
        {
            hud += $"\nEnemies in: {Mathf.CeilToInt(spawner.SetupTimeRemaining)}";
        }
        hudText.text = hud;

        if (HasGameStarted)
        {
            startButton.gameObject.SetActive(false);

        }
    }


    private void ResetPerks()
    {
        TowerController.redPerkPurchased = false;
        TowerController.yellowPerkPurchased = false;
    }
}
