using UnityEngine;
using UnityEngine.UI;

public class GameStateToggles : MonoBehaviour
{
    [SerializeField] Toggle[] gameStateToggles;

    public void UpdateGameState(GameState currentGameState)
    {
        switch (currentGameState)
        {
            case GameState.EnemySpawn:
            case GameState.VehicleSpawn:
                gameStateToggles[0].isOn = true;
                break;
            case GameState.Player:
                gameStateToggles[1].isOn = true;
                break;
            case GameState.PlayerSkill:
                gameStateToggles[2].isOn = true;
                break;
            case GameState.PreEnemy:
                gameStateToggles[3].isOn = true;
                break;
            case GameState.Vehicle:
                gameStateToggles[4].isOn = true;
                break;
        }
    }
}