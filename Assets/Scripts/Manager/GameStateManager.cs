using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameStateManager : MonoBehaviour
{
    [SerializeField] Button playerFinishButton;
    EnemySpawner enemySpawner;
    VehicleSpawner vehicleSpawner;
    LaneManager laneManager;
    UserControl userControl;
    UIMain uiMain;

    GameState gameStateIndex = GameState.Initialisation;
    bool playerTurnInProgress = false;

    int playerHealth = 10;
    int playerSkillOrb = 10;
    int damageReceived = 0;

    // Start is called before the first frame update
    void Start()
    {
        enemySpawner = gameObject.GetComponent<EnemySpawner>();
        vehicleSpawner = gameObject.GetComponent<VehicleSpawner>();
        laneManager = gameObject.GetComponent<LaneManager>();
        userControl = gameObject.GetComponent<UserControl>();

        uiMain = UIMain.Instance;
        uiMain.AddHealth(playerHealth);
        AddPlayerSkillOrb(playerSkillOrb);

        playerFinishButton.onClick.AddListener(PlayerButtonOnClick);
        StartCoroutine(CheckAndUpdateGameState(1f));
    }

    IEnumerator CheckAndUpdateGameState(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        while (true)
        {
            switch (gameStateIndex)
            {
                case GameState.Initialisation: // Initialisation phase, to transition to enemy turn
                    Debug.Log("Initialisation Complete! Starting Enemy Spawn sequence...");
                    gameStateIndex = GameState.EnemySpawn;
                    break;

                case GameState.EnemySpawn:
                    uiMain.UpdateContent();
                    enemySpawner.SpawnXEnemiesAtRandom(2);

                    Debug.Log("Spawned Enemies! Starting Vehicle Spawn sequence...");
                    gameStateIndex = GameState.VehicleSpawn;
                    break;

                case GameState.VehicleSpawn:
                    uiMain.UpdateContent();
                    vehicleSpawner.SpawnXVehiclesAtRandom(2);

                    Debug.Log("Spawned Vehicles! Starting Player's Turn...");
                    gameStateIndex = GameState.Player;
                    playerTurnInProgress = true;
                    EnableLaneSpeedToggles();
                    ResetLaneSpeedToggles();
                    AddPlayerSkillOrb(1);  // Player gets 1 skill orb every turn
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
                    vehicleSpawner.ReplacePlayerSkillVehicles();
                    Debug.Log("Finished executing Player Skills! Starting enemies turn...");

                    TriggerAllEnemiesTurn();
                    gameStateIndex = GameState.Enemy;
                    break;

                case GameState.Enemy: // Enemy Turn
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
                            CheckIfGameOver();
                            TriggerAllVehiclesTurn();
                            gameStateIndex = GameState.Vehicle;
                        }
                    }
                    break;

                case GameState.Vehicle: // Vehicle Turn
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
                            gameStateIndex = GameState.EnemySpawn;
                        }
                    }
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

    public void AddPlayerSkillOrb(int count)
    {
        userControl.AddSkillOrb(count);
    }

    private void CheckIfGameOver()
    {
        if (playerHealth <= 0)
        {
            Debug.Log("GAME OVER! YOU LOSE!");
        }
    }
}