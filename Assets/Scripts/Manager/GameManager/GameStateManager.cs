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

    int playerHealth = 1;
    int playerSkillOrb = 10;
    int damageReceived = 0;
    int enemyKilledCount = 0;
    int totalKills = 0;
    int level = 1;

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
        StartCoroutine(CheckAndUpdateGameState(0.5f));

        LoadBestScore();

        newEnemies = new Dictionary<string, Sprite>();
        for (int i = 0; i < enemyNames.Length; i++)
        {
            newEnemies.Add(enemyNames[i], enemyImgs[i]);
        }
    }

    IEnumerator CheckAndUpdateGameState(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        while (true)
        {
            switch (gameStateIndex)
            {
                case GameState.Initialisation: // Initialisation phase, to transition to enemy turn
                    if (gameStart)
                    {
                        Debug.Log("Game Started! Starting Enemy Spawn sequence...");
                        vehicleSpawner.PopulateInitialVehicles();
                        gameStateIndex = GameState.EnemySpawn;
                    }
                    break;

                case GameState.EnemySpawn:
                    uiMain.UpdateContent();
                    enemySpawner.SpawnEnemies();
                    FieldGrid.TriggerGridsRepositioning();
                    CheckAndDisplayNewEnemies();

                    Debug.Log("Spawned Enemies! Starting Vehicle Spawn sequence...");
                    gameStateIndex = GameState.VehicleSpawn;
                    break;

                case GameState.VehicleSpawn:
                    uiMain.UpdateContent();
                    vehicleSpawner.SpawnVehicles();

                    Debug.Log("Spawned Vehicles! Starting Player's Turn...");
                    gameStateIndex = GameState.Player;
                    playerTurnInProgress = true;
                    playerFinishButton.gameObject.SetActive(true);
                    EnableLaneSpeedToggles();
                    ResetLaneSpeedToggles();
                    //AddPlayerSkillOrb(1);  // Player gets 1 skill orb every turn
                    userControl.UpdateSkillTogglesFunctionality();
                    break;

                case GameState.Player:
                    uiMain.UpdateContent();

                    if (playerTurnInProgress == false)
                    {
                        DisableLaneSpeedToggles();
                        FinaliseLaneSpeed();
                        Debug.Log("Player ended turn! Starting player skills execution...");
                        userControl.DispatchSkillCommands();
                        gameStateIndex = GameState.PlayerSkill;
                    }
                    break;

                case GameState.PlayerSkill:
                    uiMain.UpdateContent();
                    userControl.RefreshSkillsUI();
                    playerSkillVehicleManager.ReplacePlayerSkillVehicles();
                    Debug.Log("Finished executing Player Skills! Starting enemies turn...");

                    TriggerAllEnemiesTurn();
                    gameStateIndex = GameState.Enemy;
                    break;

                case GameState.Enemy: // Enemy Turn
                    yield return new WaitForSeconds(waitTime);
                    uiMain.UpdateContent();
                    if (HaveUnitsOfTypeCompletedTurns("Enemy"))
                    {
                        FieldGrid.TriggerGridsRepositioning();
                        if (HaveBoostedUnitsOfType("Enemy"))
                        {
                            Debug.Log("Boosted Enemies still in play, proceeding with their next turn...");
                            TriggerBoostedUnitsOfTypeTurn("Enemy");
                            gameStateIndex = GameState.Enemy;
                        }
                        else
                        {
                            Debug.Log("All enemies turn completed! Starting vehicle's turn...");
                            UpdateRemovingHealth();
                            if (CheckIfGameOver())
                            {
                                gameStateIndex = GameState.GameOver; 
                            }
                            else
                            {
                                TriggerAllVehiclesTurn();
                                gameStateIndex = GameState.Vehicle;
                            }
                        }
                    }
                    break;

                case GameState.Vehicle: // Vehicle Turn
                    yield return new WaitForSeconds(waitTime);
                    uiMain.UpdateContent();
                    if (HaveUnitsOfTypeCompletedTurns("Vehicle"))
                    {
                        if (HaveBoostedUnitsOfType("Vehicle"))
                        {
                            Debug.Log("Boosted Vehicles still in play, proceeding with their next turn...");
                            TriggerBoostedUnitsOfTypeTurn("Vehicle");
                            gameStateIndex = GameState.Vehicle;
                        }
                        else
                        {
                            Debug.Log("All vehicles turn completed! Starting Enemy Spawn sequence...");
                            UpdateGameLevel();  // Check for update to game level after tabulating enemy killed in the last vehicle round
                            gameStateIndex = GameState.EnemySpawn;
                        }
                    }
                    break;

                case GameState.GameOver:
                    break;

                default:
                    break;
            }
            yield return new WaitForSeconds(1f);
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
        return units.Any(x => x.GetComponent<Unit>().TurnInProgress) == false;
    }

    private bool HaveBoostedUnitsOfType(string unitTag)
    {
        return GetBoostedUnitsOfType(unitTag).Length > 0;
    }

    private void TriggerBoostedUnitsOfTypeTurn(string unitTag)
    {
        var boostedUnits = GetBoostedUnitsOfType(unitTag);
        foreach (GameObject unit in boostedUnits)
        {
            unit.GetComponent<Unit>().BeginTurn();
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

    public void EnemyKilled()
    {
        totalKills += 1;
        enemyKilledCount += 1;
        AddPlayerSkillOrb(1);
        uiMain.UpdateKills(totalKills);
    }

    public void AddPlayerSkillOrb(int count)
    {
        userControl.AddSkillOrb(count);
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
            Debug.Log("GAME OVER! YOU LOSE!");
            return true;
        }
        else
        {
            return false;
        }
    }

    private void StartGame()
    {
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