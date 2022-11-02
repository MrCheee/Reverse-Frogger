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

    bool gameStart = true;
    GameState gameStateIndex = GameState.Initialisation;
    bool playerTurnInProgress = false;
    [SerializeField] string[] enemyNames;
    [SerializeField] Sprite[] enemyImgs;
    Dictionary<string, Sprite> newEnemies;

    int playerHealth = 10;
    int playerSkillOrb = 10;
    int damageReceived = 0;
    int enemyKilledCount = 0;
    int totalKills = 0;
    int level = 1;
    string difficulty = "Easy";

    float waitTime = 0.5f;
    float shortWaitTime = 0.25f;

    // Start is called before the first frame update
    void Start()
    {
        enemySpawner = gameObject.GetComponent<EndlessEnemySpawner>();
        vehicleSpawner = gameObject.GetComponent<EndlessVehicleSpawner>();
        playerSkillVehicleManager = gameObject.GetComponent<PlayerSkillVehicleManager>();
        laneManager = gameObject.GetComponent<LaneManager>();
        userControl = gameObject.GetComponent<UserControl>();

        uiMain = UIMain.Instance;
        uiMain.AddHealth(playerHealth);
        AddPlayerSkillOrb(playerSkillOrb);

        playerFinishButton.onClick.AddListener(PlayerButtonOnClick);
        startGame.onClick.AddListener(StartGame);
        StartCoroutine(CheckAndUpdateGameState());

        LoadBestScore();

        newEnemies = new Dictionary<string, Sprite>();
        //for (int i = 0; i < enemyNames.Length; i++)
        //{
        //    newEnemies.Add(enemyNames[i], enemyImgs[i]);
        //}
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
                        vehicleSpawner.PopulateInitialVehicles();

                        Debug.Log("Init --> EnemySpawn");
                        gameStateIndex = GameState.EnemySpawn;
                        uiMain.UpdateGameState(gameStateIndex);
                    }
                    break;

                case GameState.EnemySpawn:
                    uiMain.UpdateContent();
                    enemySpawner.SpawnEnemies();
                    FieldGrid.TriggerGridsRepositioning();
                    CheckAndDisplayNewEnemies();

                    Debug.Log("EnemySpawn --> VehicleSpawn");
                    gameStateIndex = GameState.VehicleSpawn;
                    break;

                case GameState.VehicleSpawn:
                    uiMain.UpdateContent();
                    vehicleSpawner.SpawnVehicles();

                    Debug.Log("VehicleSpawn --> Player");
                    gameStateIndex = GameState.Player;
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
                    userControl.RefreshSkillsUI();
                    playerSkillVehicleManager.ReplacePlayerSkillVehicles();

                    Debug.Log("Player --> PreEnemy");
                    TriggerAllEnemiesPreTurn();
                    gameStateIndex = GameState.PreEnemy;
                    uiMain.UpdateGameState(gameStateIndex);
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
                        if (HaveBoostedUnitsOfType("Enemy"))
                        {
                            Debug.Log("PostEnemy --> BoostedPreEnemy");
                            TriggerBoostedUnitsOfTypePreTurn("Enemy");
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
                            }
                        }
                    }

                    break;

                case GameState.BoostedPreEnemy:
                    yield return new WaitForSeconds(shortWaitTime);
                    uiMain.UpdateContent();

                    if (HaveUnitsOfTypeCompletedTurns("Enemy"))
                    {
                        Debug.Log("BoostedPreEnemy --> BoostedEnemy");
                        TriggerBoostedUnitsOfTypeTurn("Enemy");
                        gameStateIndex = GameState.BoostedEnemy;
                    }
                    break;

                case GameState.BoostedEnemy:
                    yield return new WaitForSeconds(waitTime);
                    uiMain.UpdateContent();

                    if (HaveUnitsOfTypeCompletedTurns("Enemy"))
                    {
                        Debug.Log("BoostedEnemy --> PostEnemy");
                        TriggerBoostedUnitsOfTypePostTurn("Enemy");
                        ReduceBoostOnUnitsOfType("Enemy");
                        gameStateIndex = GameState.PostEnemy;
                    }
                    break;

                case GameState.Vehicle: // Vehicle Turn
                    yield return new WaitForSeconds(waitTime);
                    uiMain.UpdateContent();

                    if (HaveUnitsOfTypeCompletedTurns("Vehicle"))
                    {
                        if (HaveBoostedUnitsOfType("Vehicle"))
                        {
                            Debug.Log("Vehicle --> Vehicle (Boosted)");
                            TriggerBoostedUnitsOfTypeTurn("Vehicle");
                            ReduceBoostOnUnitsOfType("Vehicle");
                            gameStateIndex = GameState.Vehicle;
                        }
                        else
                        {
                            Debug.Log("Vehicle --> EnemySpawn");
                            UpdateGameLevel();  // Check for update to game level after tabulating enemy killed in the last vehicle round
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
            veh.UpdateLaneSpeed(vehLaneSpeed);
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
            if (newEnemies.ContainsKey(enemyName))
            {
                uiMain.DisplayNewEnemy(enemyUnit, newEnemies[enemyName]);
                newEnemies.Remove(enemyName);
            }
        }
    }

    private bool HaveUnitsOfTypeCompletedTurns(string unitTag)
    {
        var units = GameObject.FindGameObjectsWithTag(unitTag);
        bool issuedAllMovement = units.Any(x => x.GetComponent<Unit>().TurnInProgress) == false;
        bool completedFinalMovement = units.All(x => x.GetComponent<Unit>().CheckIfCompletedPreviousMovement());
        return issuedAllMovement && completedFinalMovement;
    }

    private bool HaveBoostedUnitsOfType(string unitTag)
    {
        return GetBoostedUnitsOfType(unitTag).Length > 0;
    }

    private void TriggerBoostedUnitsOfTypePreTurn(string unitTag)
    {
        var boostedUnits = GetBoostedUnitsOfType(unitTag);
        foreach (GameObject unit in boostedUnits)
        {
            unit.GetComponent<Unit>().BeginPreTurn();
        }
    }

    private void TriggerBoostedUnitsOfTypeTurn(string unitTag)
    {
        var boostedUnits = GetBoostedUnitsOfType(unitTag);
        foreach (GameObject unit in boostedUnits)
        {
            unit.GetComponent<Unit>().BeginTurn();
        }
    }

    private void TriggerBoostedUnitsOfTypePostTurn(string unitTag)
    {
        var boostedUnits = GetBoostedUnitsOfType(unitTag);
        foreach (GameObject unit in boostedUnits)
        {
            unit.GetComponent<Unit>().BeginPostTurn();
        }
    }

    private void ReduceBoostOnUnitsOfType(string unitTag)
    {
        var boostedUnits = GetBoostedUnitsOfType(unitTag);
        foreach (GameObject unit in boostedUnits)
        {
            unit.GetComponent<Unit>().ReduceBoostOnUnit();
        }
    }

    private GameObject[] GetBoostedUnitsOfType(string unitTag)
    {
        return GameObject.FindGameObjectsWithTag(unitTag).Where(x => x.GetComponent<Unit>().isBoosted()).ToArray();
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

    public void EnemyKilled(Vector3 killedPos, string killedInfo)
    {
        totalKills += 1;
        enemyKilledCount += 1;
        AddPlayerSkillOrb(1);
        uiMain.UpdateKills(totalKills);
        uiMain.UpdateGameLog(killedInfo);

        uiMain.DisplayKilledEnemy(killedPos);
    }

    public void AddPlayerSkillOrb(int count)
    {
        userControl.AddSkillOrb(count);
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
        while (enemyKilledCount >= 5)
        {
            enemyKilledCount -= 5;
            level += 1;
            uiMain.UpdateLevel(level);
            enemySpawner.IncrementLevel();
            vehicleSpawner.IncrementLevel();
        }
    }

    private bool CheckIfGameOver()
    {
        if (playerHealth <= 0)
        {
            SaveBestScore();
            uiMain.DisplayGameOver();
            uiMain.UpdateGameLog("GAME OVER! The frogs have conquered humanity!");
            //Debug.Log("GAME OVER! YOU LOSE!");
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
        SaveData data;
        if (File.Exists(path))
        {
            string jsonIn = File.ReadAllText(path);
            data = JsonUtility.FromJson<SaveData>(jsonIn);

            if (totalKills > data.kills)
            {
                data.kills = totalKills;
            }
        }
        else
        {
            data = new SaveData();
            data.kills = totalKills;
        }
        
        string jsonOut = JsonUtility.ToJson(data);
        File.WriteAllText(Application.persistentDataPath + "/highscore.json", jsonOut);
    }

    public void LoadBestScore()
    {
        string path = Application.persistentDataPath + "/highscore.json";
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            uiMain.UpdateBestScore(data.kills);
        }
        else
        {
            uiMain.UpdateBestScore(0);
        }
    }

    [System.Serializable]
    class SaveData
    {
        public int kills;
    }
}