using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameStateManager : MonoBehaviour
{
    [SerializeField] Button playerFinishButton;
    [SerializeField] Button startGame;
    [SerializeField] ToggleGroup difficultyToggle;
    IEnemySpawner enemySpawner;
    IVehicleSpawner vehicleSpawner;
    IPlayerSkillVehicleManager playerSkillVehicleManager;
    LaneManager laneManager;
    UserControl userControl;
    UIMain uiMain;

    GameStateSoundManager gameStateSoundManager;
    [SerializeField] AudioSource BGMAudioSource;
    [SerializeField] AudioClip BGMMusic;
    [SerializeField] AudioClip GameOverMusic;
    [SerializeField] AudioSource LevelUpAudioSource;

    [SerializeField] GameObject EnemyKilledPrefab;

    bool gameStart = false;
    GameState gameStateIndex = GameState.Initialisation;
    bool playerTurnInProgress = false;
    HashSet<string> newEnemies;

    int playerHealth = 10;
    int playerSkillOrb = 0;
    int damageReceived = 0;
    int orbReceived = 0;
    int enemyKilledCount = 0;
    int totalKills = 0;
    int level = 1;
    string difficulty = "Advanced";
    bool altWave = false;

    float waitTime = .5f;
    float shortWaitTime = 0.25f;
    float gameStartWaitTime = 1f;

    Unit boostedUnit = null;

    // Start is called before the first frame update
    void Start()
    {
        enemySpawner = gameObject.GetComponent<EndlessEnemySpawner>();
        vehicleSpawner = gameObject.GetComponent<EndlessVehicleSpawner>();
        playerSkillVehicleManager = gameObject.GetComponent<PlayerSkillVehicleManager>();
        gameStateSoundManager = GameObject.Find("GameStateSoundManager").GetComponent<GameStateSoundManager>();
        laneManager = gameObject.GetComponent<LaneManager>();
        userControl = gameObject.GetComponent<UserControl>();

        uiMain = UIMain.Instance;

        startGame.onClick.AddListener(StartGame);
        playerFinishButton.onClick.AddListener(PlayerButtonOnClick);
        StartCoroutine(CheckAndUpdateGameState());

        newEnemies = new HashSet<string>() { 
            "Blob", 
            "Ghoul", 
            "Killer Crab", 
            "Charger",  
            "Imp", 
            "Mutated Vaulter",
            "Minotaur",
            "Shield Warrior", 
            "Mutated Brain", 
            "Bloat", 
            "Mutated Blood" 
        };

        // Events Subscribing
        Enemy.OnEnemyKilled += EnemyKilled;
        Enemy.DamagePlayer += DamagePlayer;
        BoostUnit.BoostedUnit += RegisterBoostedUnit;
    }

    private void OnDisable()
    {
        // Events Unsubscribing
        Enemy.OnEnemyKilled -= EnemyKilled;
        Enemy.DamagePlayer -= DamagePlayer;
    }

    IEnumerator CheckAndUpdateGameState()
    {
        yield return new WaitForSeconds(waitTime);

        while (true)
        {
            switch (gameStateIndex)
            {
                case GameState.Initialisation: // Initialisation phase, to transition to enemy turn
                    if (gameStart)
                    {
                        InitialiseDifficulty();
                        uiMain.Initialise(playerHealth, playerSkillOrb);
                        LoadBestScore();
                        vehicleSpawner.PopulateInitialVehicles();
                        BGMAudioSource.clip = BGMMusic;
                        BGMAudioSource.pitch = 0.75f;
                        BGMAudioSource.Play();

                        Debug.Log("Init --> EnemySpawn");
                        gameStateIndex = GameState.EnemySpawn;
                        uiMain.UpdateGameState(gameStateIndex);
                        yield return new WaitForSeconds(gameStartWaitTime);
                    }
                    break;

                case GameState.EnemySpawn:
                    uiMain.UpdateContent();
                    gameStateSoundManager.PlayEnemySpawnGameState();
                    enemySpawner.SpawnEnemies();
                    FieldGrid.TriggerGridsRepositioning();
                    
                    Debug.Log("EnemySpawn --> VehicleSpawn");
                    gameStateIndex = GameState.VehicleSpawn;
                    break;

                case GameState.VehicleSpawn:
                    uiMain.UpdateContent();
                    vehicleSpawner.SpawnVehicles();

                    Debug.Log("VehicleSpawn --> Player");
                    gameStateIndex = GameState.Player;
                    userControl.RefreshSkillsUI();
                    CheckAndDisplayNewEnemies();
                    playerTurnInProgress = true;
                    playerFinishButton.gameObject.SetActive(true);
                    EnableLaneSpeedToggles();
                    ResetLaneSpeedToggles();
                    userControl.UpdateSkillTogglesFunctionality();
                    uiMain.UpdateGameState(gameStateIndex);
                    break;

                case GameState.Player:
                    uiMain.UpdateContent();

                    if (playerTurnInProgress == false)
                    {
                        uiMain.ResetHealthAndSkillGainInfo();
                        userControl.DisableSkillBar();
                        DisableLaneSpeedToggles();
                        FinaliseLaneSpeed();
                        RemoveEnemyKilledIndicators();
                        Debug.Log("Player --> PlayerSkill");
                        userControl.DispatchSkillCommands();
                        gameStateIndex = GameState.PlayerSkill;
                        uiMain.UpdateGameState(gameStateIndex);
                    }
                    break;

                case GameState.PlayerSkill:
                    uiMain.UpdateContent();

                    if (userControl.DispatchSkillCompleted())
                    {
                        playerSkillVehicleManager.ReplacePlayerSkillVehicles();

                        Debug.Log("PlayerSkill --> PreEnemy");
                        TriggerAllEnemiesPreTurn();
                        gameStateIndex = GameState.PreEnemy;
                        uiMain.UpdateGameState(gameStateIndex);
                    }
                    break;

                case GameState.PreEnemy:
                    yield return new WaitForSeconds(shortWaitTime);
                    uiMain.UpdateContent();

                    if (HaveUnitsOfTypeCompletedTurns("Enemy"))
                    {
                        Debug.Log("PreEnemy --> Enemy");
                        TriggerAllEnemiesTurn();
                        gameStateIndex = GameState.Enemy;
                    }
                    break;

                case GameState.Enemy:
                    yield return new WaitForSeconds(waitTime);
                    uiMain.UpdateContent();
                    if (HaveUnitsOfTypeCompletedTurns("Enemy"))
                    {
                        Debug.Log("Enemy --> PostEnemy");
                        TriggerAllEnemiesPostTurn();
                        gameStateIndex = GameState.PostEnemy;
                    }
                    break;

                case GameState.PostEnemy:
                    yield return new WaitForSeconds(shortWaitTime);
                    FieldGrid.TriggerGridsRepositioning();

                    if (HaveUnitsOfTypeCompletedTurns("Enemy"))
                    {
                        if (boostedUnit != null && boostedUnit is Enemy)
                        {
                            Debug.Log("PostEnemy --> BoostedPreEnemy");
                            TriggerBoostedUnitPreTurn();
                            gameStateIndex = GameState.BoostedPreEnemy;
                        }
                        else
                        {
                            UpdateRemovingHealth();
                            if (CheckIfGameOver())
                            {
                                Debug.Log("PostEnemy --> GameOver");
                                gameStateIndex = GameState.GameOver;
                            }
                            else
                            {
                                Debug.Log("PostEnemy --> Vehicle");
                                TriggerAllVehiclesTurn();
                                gameStateIndex = GameState.Vehicle;
                                uiMain.UpdateGameState(gameStateIndex);
                                gameStateSoundManager.PlayVehicleGameState();
                            }
                        }
                    }

                    break;

                case GameState.BoostedPreEnemy:
                    yield return new WaitForSeconds(shortWaitTime);
                    uiMain.UpdateContent();

                    if (!boostedUnit.TurnInProgress)
                    {
                        Debug.Log("BoostedPreEnemy --> BoostedEnemy");
                        TriggerBoostedUnitTurn();
                        gameStateIndex = GameState.BoostedEnemy;
                    }
                    break;

                case GameState.BoostedEnemy:
                    yield return new WaitForSeconds(waitTime);
                    uiMain.UpdateContent();

                    if (!boostedUnit.TurnInProgress)
                    {
                        Debug.Log("BoostedEnemy --> PostEnemy");
                        TriggerBoostedUnitPostTurn();
                        boostedUnit = null;
                        gameStateIndex = GameState.PostEnemy;
                    }
                    break;

                case GameState.Vehicle: // Vehicle Turn
                    yield return new WaitForSeconds(waitTime);
                    uiMain.UpdateContent();

                    if (HaveUnitsOfTypeCompletedTurns("Vehicle"))
                    {
                        if (boostedUnit != null && boostedUnit is Vehicle)
                        {
                            Debug.Log("Vehicle --> Vehicle (Boosted)");
                            gameStateSoundManager.PlayVehicleGameState();
                            TriggerBoostedUnitTurn();
                            boostedUnit = null;
                            gameStateIndex = GameState.Vehicle;
                        }
                        else
                        {
                            Debug.Log("Vehicle --> EnemySpawn");
                            UpdateGameLevel();  // Check for update to game level after tabulating enemy killed in the last vehicle round
                            uiMain.DisplaySkillGain(orbReceived);
                            orbReceived = 0;
                            gameStateIndex = GameState.EnemySpawn;
                            uiMain.UpdateGameState(gameStateIndex);
                        }
                    }
                    break;

                case GameState.GameOver:
                    break;

                default:
                    break;
            }
            yield return new WaitForSeconds(waitTime);
        }
    }

    private void TriggerAllEnemiesTurn()
    {
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            enemy.GetComponent<Enemy>().BeginTurn();
        }
    }

    private void TriggerAllEnemiesPreTurn()
    {
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            enemy.GetComponent<Enemy>().BeginPreTurn();
        }
    }

    private void TriggerAllEnemiesPostTurn()
    {
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            enemy.GetComponent<Enemy>().BeginPostTurn();
        }
    }

    private void TriggerAllVehiclesTurn()
    {
        var vehicles = GameObject.FindGameObjectsWithTag("Vehicle");
        foreach (GameObject vehObj in vehicles)
        {
            Vehicle veh = vehObj.GetComponent<Vehicle>();
            int vehLaneSpeed = laneManager.GetLaneSpeed(veh.GetCurrentHeadGridPosition().y);
            veh.UpdateLaneSpeedAddition(vehLaneSpeed);
            veh.BeginTurn();
        }
    }

    private void CheckAndDisplayNewEnemies()
    {
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            Enemy enemyUnit = enemy.GetComponent<Enemy>();
            string enemyName = enemyUnit.GetName();
            if (newEnemies.Contains(enemyName))
            {
                uiMain.DisplayNewEnemy(enemyUnit);
                newEnemies.Remove(enemyName);
            }
        }
    }

    private bool HaveUnitsOfTypeCompletedTurns(string unitTag)
    {
        var units = GameObject.FindGameObjectsWithTag(unitTag);
        return units.Any(x => x.GetComponent<Unit>().TurnInProgress) == false;
    }

    private void RegisterBoostedUnit(Unit unit)
    {
        boostedUnit = unit;
    }

    private void TriggerBoostedUnitPreTurn()
    {
        boostedUnit?.GetComponent<Unit>().BeginPreTurn();
    }

    private void TriggerBoostedUnitTurn()
    {
        boostedUnit?.GetComponent<Unit>().BeginTurn();
    }

    private void TriggerBoostedUnitPostTurn()
    {
        boostedUnit?.GetComponent<Unit>().BeginPostTurn();
    }

    private void EnableLaneSpeedToggles()
    {
        foreach (GameObject toggleObj in GameObject.FindGameObjectsWithTag("SpeedToggle"))
        {
            toggleObj.GetComponent<Toggle>().enabled = true;
        }
    }

    private void DisableLaneSpeedToggles()
    {
        foreach (GameObject toggleObj in GameObject.FindGameObjectsWithTag("SpeedToggle"))
        {
            toggleObj.GetComponent<Toggle>().enabled = false;
        }
    }

    private void ResetLaneSpeedToggles()
    {
        foreach (GameObject toggleObj in GameObject.FindGameObjectsWithTag("SpeedToggle"))
        {
            if (toggleObj.name == "SpeedSame")
            {
                toggleObj.GetComponent<Toggle>().SetIsOnWithoutNotify(true);
            }
        }
    }

    private void FinaliseLaneSpeed()
    {
        foreach (LaneControl lane in FindObjectsOfType<LaneControl>())
        {
            lane.FinaliseSpeed();
        }
    }

    private void PlayerButtonOnClick()
    {
        playerTurnInProgress = false;
        playerFinishButton.gameObject.SetActive(false);
    }

    public void DamagePlayer(int damage)
    {
        playerHealth -= damage;
        damageReceived += damage;
    }

    private void UpdateRemovingHealth()
    {
        uiMain.RemoveHealth(damageReceived);
        damageReceived = 0;
    }

    public void EnemyKilled(Enemy enemy)
    {
        GridCoord killedPos = enemy.GetCurrentHeadGridPosition();
        Debug.Log($"{enemy.GetName()} has been killed at Grid [{killedPos.x}, {killedPos.y}]!");
        totalKills += 1;
        enemyKilledCount += 1;
        orbReceived += 1;

        // ** Can shift out to somewhr else?
        Instantiate(EnemyKilledPrefab, enemy.gameObject.transform.position, EnemyKilledPrefab.transform.rotation);
    }

    private void RemoveEnemyKilledIndicators()
    {
        var indicators = GameObject.FindGameObjectsWithTag("EnemyKilledIndicator");
        foreach (GameObject ind in indicators)
        {
            Destroy(ind);
        }
    }

    public void UpdateGameLevel()
    {
        if (difficulty == "Advanced")
        {
            if (level < 5)
            {
                LevelUp();
            }
            else if (level >= 5 && level < 10)
            {
                if (altWave)
                {
                    LevelUp();
                    altWave = false;
                }
                else
                {
                    altWave = true;
                }
            }
        }

        while (enemyKilledCount >= 5)
        {
            enemyKilledCount -= 5;
            LevelUp();
        }
    }

    private void LevelUp()
    {
        level += 1;
        uiMain.UpdateLevel(level);
        enemySpawner.IncrementLevel();
        vehicleSpawner.IncrementLevel();
        LevelUpAudioSource.Play(); ;
    }

    private bool CheckIfGameOver()
    {
        if (playerHealth <= 0)
        {
            BGMAudioSource.clip = GameOverMusic;
            BGMAudioSource.Play();
            SaveBestScore();
            uiMain.DisplayGameOver();
            //uiMain.UpdateGameLog("GAME OVER! The frogs have conquered humanity!");
            return true;
        }
        else
        {
            return false;
        }
    }

    private void InitialiseDifficulty()
    {
        Debug.Log(difficulty);
        enemySpawner.SetDifficulty(difficulty);
    }

    private void StartGame()
    {
        difficulty = difficultyToggle.ActiveToggles().First().name;
        uiMain.CloseStartMenu();
        gameStart = true;       
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(0);
    }

    public void SaveBestScore()
    {
        string path = Application.persistentDataPath + "/highscore.json";
        SaveData killsByDifficulty;
        int difficultyIndex = 0;
        if (difficulty == "Advanced") difficultyIndex = 1;

        if (File.Exists(path))
        {
            string jsonIn = File.ReadAllText(path);
            killsByDifficulty = JsonUtility.FromJson<SaveData>(jsonIn);

            if (totalKills > killsByDifficulty.kills[difficultyIndex])
            {
                killsByDifficulty.kills[difficultyIndex] = totalKills;
            }
        }
        else
        {
            Debug.Log("Creating new dict...");
            killsByDifficulty.kills = new int[3] { 0, 0, 0 };
            killsByDifficulty.kills[difficultyIndex] = totalKills;
        }

        Debug.Log(killsByDifficulty.kills);
        string jsonOut = JsonUtility.ToJson(killsByDifficulty);
        Debug.Log(jsonOut);
        File.WriteAllText(Application.persistentDataPath + "/highscore.json", jsonOut);
    }

    public void LoadBestScore()
    {
        string path = Application.persistentDataPath + "/highscore.json";
        int difficultyIndex = 0;
        if (difficulty == "Advanced") difficultyIndex = 1;

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            SaveData killsByDifficulty = JsonUtility.FromJson<SaveData>(json);

            uiMain.UpdateBestScore(difficulty, killsByDifficulty.kills[difficultyIndex]);
        }
        else
        {
            uiMain.UpdateBestScore(difficulty, 0);
        }
    }

    [System.Serializable]
    struct SaveData
    {
        public int[] kills;
    }
}
