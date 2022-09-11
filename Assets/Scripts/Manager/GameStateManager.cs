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
    UIMain uiMain;

    GameState gameStateIndex = GameState.Initialisation;
    bool playerTurnInProgress = false;

    // Start is called before the first frame update
    void Start()
    {
        enemySpawner = gameObject.GetComponent<EnemySpawner>();
        vehicleSpawner = gameObject.GetComponent<VehicleSpawner>();
        laneManager = gameObject.GetComponent<LaneManager>();

        uiMain = UIMain.Instance;

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
                    var enemies = GameObject.FindGameObjectsWithTag("Enemy");
                    var vehicles = GameObject.FindGameObjectsWithTag("Vehicle");
                    Debug.Log("Initialisation Complete...");
                    gameStateIndex = GameState.EnemySpawn;
                    break;
                case GameState.Enemy: // Enemy Turn
                    uiMain.UpdateContent();
                    enemies = GameObject.FindGameObjectsWithTag("Enemy");
                    if (enemies.Any(x => x.GetComponent<Unit>().TurnInProgress) == false)
                    {
                        Debug.Log("All enemies turn completed...");

                        FieldGrid.TriggerGridsRepositioning();

                        vehicles = GameObject.FindGameObjectsWithTag("Vehicle");
                        foreach (GameObject vehObj in vehicles)
                        {
                            Vehicle veh = vehObj.GetComponent<Vehicle>();
                            int vehLaneSpeed = laneManager.GetLaneSpeed(veh.GetCurrentGridPosition().y);
                            veh.UpdateLaneSpeed(vehLaneSpeed);
                            veh.BeginTurn();
                        }
                        Debug.Log("Started vehicles turn...");
                        gameStateIndex = GameState.Vehicle;
                    }
                    break;
                case GameState.Vehicle: // Vehicle Turn
                    uiMain.UpdateContent();
                    vehicles = GameObject.FindGameObjectsWithTag("Vehicle");
                    if (vehicles.Any(x => x.GetComponent<Unit>().TurnInProgress) == false)
                    {
                        Debug.Log("All vehicles turn completed...");
                        Debug.Log("Started Enemy Spawn sequence...");
                        gameStateIndex = GameState.EnemySpawn;
                    }
                    break;
                case GameState.EnemySpawn:
                    uiMain.UpdateContent();
                    enemySpawner.SpawnXEnemiesAtRandom(2);
                    gameStateIndex = GameState.VehicleSpawn;
                    Debug.Log("Spawned Enemies! Starting Vehicle Spawn sequence...");
                    break;
                case GameState.VehicleSpawn:
                    uiMain.UpdateContent();
                    vehicleSpawner.SpawnXVehiclesAtRandom(2);
                    gameStateIndex = GameState.Player;
                    Debug.Log("Spawned Vehicles! Starting Player's Turn...");
                    foreach (GameObject toggleObj in GameObject.FindGameObjectsWithTag("SpeedToggle"))
                    {
                        toggleObj.GetComponent<Toggle>().enabled = true;
                    }
                    foreach (GameObject toggleObj in GameObject.FindGameObjectsWithTag("SpeedToggle"))
                    {
                        if (toggleObj.name == "SpeedSame")
                        {
                            toggleObj.GetComponent<Toggle>().SetIsOnWithoutNotify(true);
                        }
                    }
                    playerTurnInProgress = true;
                    break;
                case GameState.Player:
                    uiMain.UpdateContent();
                    if (playerTurnInProgress == false)
                    {
                        Debug.Log("Player ended turn...");
                        foreach (LaneControl lane in FindObjectsOfType<LaneControl>())
                        {
                            lane.FinaliseSpeed();
                        }
                        foreach (GameObject toggleObj in GameObject.FindGameObjectsWithTag("SpeedToggle"))
                        {
                            toggleObj.GetComponent<Toggle>().enabled = false;
                        }
                        Debug.Log("Started player skills...");
                        gameStateIndex = GameState.PlayerSkill;
                    }
                    break;
                case GameState.PlayerSkill:
                    uiMain.UpdateContent();
                    Debug.Log("Finished Player Skills... Starting enemies turn...");
                    enemies = GameObject.FindGameObjectsWithTag("Enemy");
                    foreach (GameObject enemy in enemies)
                    {
                        enemy.GetComponent<Enemy>().BeginTurn();
                    }
                    gameStateIndex = GameState.Enemy;
                    break;
                default:
                    break;
            }
            yield return new WaitForSeconds(1f);
        }
        
    }

    void PlayerButtonOnClick()
    {
        playerTurnInProgress = false;
    }
}