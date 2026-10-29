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

    public TextMeshProUGUI countdownText;
    public TextMeshProUGUI waveText;
    public TextMeshProUGUI damageText;
    public TextMeshProUGUI hudText;
    public TextMeshProUGUI messageText;

    public EnemySpawner spawner;
    public Button startButton;
    public GameObject skillTree;

    public int Currency { get; set; }
    public int BaseHealth { get; private set; }
    public bool IsGameOver { get; private set; }
    public bool HasGameStarted { get; private set; }

    public bool HasWaveStarted { get; private set; }
    public int redDamage { get; private set; }
    public int redDamageHid { get; set; }
    public int yellowDamage { get; private set; }
    public int yellowDamageHid { get; set; }
    public int currentWave { get; private set; }

    private Button MainMenuButton;
    private Button RestartButton;
    public GameObject GameOverButtons;



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

        MainMenuButton = GameOverButtons.GetComponentsInChildren<Button>(true)[0];
        RestartButton = GameOverButtons.GetComponentsInChildren<Button>(true)[1];
        
        startButton.onClick.AddListener(StartGame);
        MainMenuButton.onClick.AddListener(GoToMainMenu);
        RestartButton.onClick.AddListener(RestartLevel);

        GameOverButtons.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateCountdown();
        UpdateWave();
        UpdateDamage();
        UpdateHud();

        if (IsGameOver)
        {
            GameOverButtons.SetActive(true);
            return;
        } else
        {
            GameOverButtons.SetActive(false);
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
            redDamageHid++;
        }
        if (type == "yellow")
        {
            yellowDamage ++;
            yellowDamageHid++;
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

    //Make spawner not ready, call to reset the spawner.
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
                messageText.text = "YOU WIN!";
            } else
            {
                messageText.text = "BASE DESTROYED";
            }
        }
        messageTimer = 0;
    }

    // Updates enemy countdown text
    private void UpdateCountdown()
    {
        if (countdownText == null) return;
        string countdown = "";
        if (spawner != null && spawner.SetupTimeRemaining > 0)
        {
            countdown += $"\nEnemies in: {Mathf.CeilToInt(spawner.SetupTimeRemaining)}";
        }
        countdownText.text = countdown;
    }

    // Updates wave text
    private void UpdateWave()
    {
        if (waveText == null) return;
        string wave = $"Wave: {currentWave + 1}";
        waveText.text = wave;
    }

    // Updates damage text
    private void UpdateDamage()
    {
        if (damageText == null) return;
        string damage = $"Red: {redDamage}\nYellow: {yellowDamage}";
        damageText.text = damage;
    }

    // Updates hud text
    private void UpdateHud()
    {
        if (hudText == null) return;
        string hud = $"Gold: {Currency}\nBase HP: {BaseHealth}";
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
        TowerController.redPerkPurchased1 = false;
        TowerController.yellowPerkPurchased1 = false;
    }

    private void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        ResetPerks();
    }

    private void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }
}
