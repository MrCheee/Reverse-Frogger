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

    GameState gameStateIndex = GameState.Initialisation;
    bool playerTurnInProgress = false;

    // Start is called before the first frame update
    void Start()
    {
        enemySpawner = gameObject.GetComponent<EnemySpawner>();
        vehicleSpawner = gameObject.GetComponent<VehicleSpawner>();

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
                    enemies = GameObject.FindGameObjectsWithTag("Enemy");
                    if (enemies.Any(x => x.GetComponent<Unit>().TurnInProgress) == false)
                    {
                        Debug.Log("All enemies turn completed...");
                        vehicles = GameObject.FindGameObjectsWithTag("Vehicle");
                        foreach (GameObject veh in vehicles)
                        {
                            veh.GetComponent<Vehicle>().BeginTurn();
                        }
                        Debug.Log("Started vehicles turn...");
                        gameStateIndex = GameState.Vehicle;
                    }
                    break;
                case GameState.Vehicle: // Vehicle Turn
                    vehicles = GameObject.FindGameObjectsWithTag("Vehicle");
                    if (vehicles.Any(x => x.GetComponent<Unit>().TurnInProgress) == false)
                    {
                        Debug.Log("All vehicles turn completed...");
                        Debug.Log("Started Enemy Spawn sequence...");
                        gameStateIndex = GameState.EnemySpawn;
                    }
                    break;
                case GameState.EnemySpawn:
                    enemySpawner.SpawnXEnemiesAtRandom(2);
                    gameStateIndex = GameState.VehicleSpawn;
                    Debug.Log("Spawned Enemies! Starting Vehicle Spawn sequence...");
                    break;
                case GameState.VehicleSpawn:
                    vehicleSpawner.SpawnXVehiclesAtRandom(2);
                    gameStateIndex = GameState.Player;
                    Debug.Log("Spawned Vehicles! Starting Player's Turn...");
                    playerTurnInProgress = true;
                    break;
                case GameState.Player:
                    if (playerTurnInProgress == false)
                    {
                        Debug.Log("Player ended turn...");
                        enemies = GameObject.FindGameObjectsWithTag("Enemy");
                        foreach (GameObject enemy in enemies)
                        {
                            enemy.GetComponent<Enemy>().BeginTurn();
                        }
                        Debug.Log("Started all enemies turn...");
                        gameStateIndex = GameState.Enemy;
                    }
                    break;
                case GameState.PlayerSkill:
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