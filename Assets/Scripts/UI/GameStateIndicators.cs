using UnityEngine;
using UnityEngine.UI;

public class GameStateIndicators : MonoBehaviour
{
    [SerializeField] GameObject[] gameStateIndicators;

    public void UpdateGameState(GameState currentGameState)
    {
        switch (currentGameState)
        {
            case GameState.EnemySpawn:
            case GameState.VehicleSpawn:
                gameStateIndicators[4].SetActive(false);
                gameStateIndicators[0].SetActive(true);
                break;
            case GameState.Player:
                gameStateIndicators[0].SetActive(false);
                gameStateIndicators[1].SetActive(true);
                break;
            case GameState.PlayerSkill:
                gameStateIndicators[1].SetActive(false);
                gameStateIndicators[2].SetActive(true);
                break;
            case GameState.PreEnemy:
                gameStateIndicators[2].SetActive(false);
                gameStateIndicators[3].SetActive(true);
                break;
            case GameState.Vehicle:
                gameStateIndicators[3].SetActive(false);
                gameStateIndicators[4].SetActive(true);
                break;
        }
    }
}