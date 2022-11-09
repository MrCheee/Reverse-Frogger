using System.Collections.Generic;
using UnityEngine;

public class EndlessEnemySpawner : MonoBehaviour, IEnemySpawner
{
    [SerializeField] private GameObject[] enemyPrefabs;
    private GameLogWindow gameLogWindow;

    List<EnemyType> _currentSpawnList;
    int spawnY = FieldGrid.GetMaxHeight() - FieldGrid.GetFieldBuffer() - 1;
    int spawnXMin = FieldGrid.GetFieldBuffer() + 2;
    int spawnXMax = FieldGrid.GetMaxLength() - FieldGrid.GetFieldBuffer() - 2;
    int level;
    int spawnCount;
    EnemyType forcedSpawn = EnemyType.None;

    // Start is called before the first frame update
    void Start()
    {
        gameLogWindow = GameObject.Find("GameLogWindow").GetComponent<GameLogWindow>();
    }

    public void SetDifficulty(string difficultyLevel)
    {
        forcedSpawn = EnemyType.Soldier;
        level = 1;
        if (difficultyLevel == "Hard")
        {
            _currentSpawnList = new List<EnemyType>()
            {
                EnemyType.Soldier
            };
            spawnCount = 3;
        }
        else if (difficultyLevel == "Medium")
        {
            _currentSpawnList = new List<EnemyType>()
            {
                EnemyType.Soldier
            };
            spawnCount = 2;
        }
        else
        {
            _currentSpawnList = new List<EnemyType>()
            {
                EnemyType.Soldier,
                EnemyType.Soldier
            };
            spawnCount = 2;
        }
    }

    public void SpawnEnemies()
    {
        List<int> reservedSpawnX = new List<int>();
        int enemyCount = Mathf.Min(spawnCount, spawnXMax - spawnXMin);

        for (int i = 0; i < enemyCount; i++)
        {
            EnemyType enemyIndex;
            if (forcedSpawn != EnemyType.None)
            {
                enemyIndex = forcedSpawn;
                forcedSpawn = EnemyType.None;
            }
            else
            {
                enemyIndex = _currentSpawnList[Random.Range(0, _currentSpawnList.Count)];
            }
            int spawnX = RandomNonRepeating(reservedSpawnX, spawnXMin, spawnXMax);
            reservedSpawnX.Add(spawnX);
            GridCoord spawnGrid = new GridCoord(spawnX, spawnY);
            Vector3 spawnPos = FieldGrid.GetSingleGrid(spawnGrid).GetGridCentrePoint();
            GameObject enemy = Instantiate(enemyPrefabs[(int)enemyIndex], spawnPos, enemyPrefabs[(int)enemyIndex].transform.rotation);
            enemy.GetComponent<Enemy>().AddToFieldGridPosition(spawnGrid);
            gameLogWindow.AddToGameLog($"{enemy.GetComponent<Unit>().GetName()} has spawned at Grid [{spawnGrid.x}, {spawnGrid.y}]!");
        }
    }

    public void IncrementLevel()
    {
        level += 1;
        ExecuteLevelUpActions();
    }

    public void ReduceLevel()
    {
        return;
    }

    int RandomNonRepeating(List<int> selectedNumbers, int start, int end)
    {
        int selected;
        do
        {
            selected = Random.Range(start, end);
        } while (selectedNumbers.Contains(selected));
        return selected;
    }

    void ExecuteLevelUpActions()
    {
        switch (level)
        {
            case 2:
                _currentSpawnList.Add(EnemyType.Skater);
                forcedSpawn = EnemyType.Skater;
                break;
            case 3:
                _currentSpawnList.Add(EnemyType.Sprinter);
                forcedSpawn = EnemyType.Sprinter;
                break;
            case 4:
                _currentSpawnList.Add(EnemyType.Charger);
                forcedSpawn = EnemyType.Charger;
                break;
            case 5:
                _currentSpawnList.Add(EnemyType.Brute);
                forcedSpawn = EnemyType.Brute;
                break;
            case 6:
                _currentSpawnList.Add(EnemyType.Vaulter);
                forcedSpawn = EnemyType.Vaulter;
                break;
            case 7:
                _currentSpawnList.Add(EnemyType.Bloat);
                forcedSpawn = EnemyType.Bloat;
                break;
            case 8:
                _currentSpawnList.Add(EnemyType.Jumper);
                forcedSpawn = EnemyType.Jumper;
                break;
            case 9:
                _currentSpawnList.Add(EnemyType.Flatten);
                forcedSpawn = EnemyType.Flatten;
                break;
            case 10:
                _currentSpawnList.Add(EnemyType.BabyForesight);
                _currentSpawnList.Add(EnemyType.Brute);
                forcedSpawn = EnemyType.BabyForesight;
                break;
            case 11:
                _currentSpawnList.Add(EnemyType.LShield);
                forcedSpawn = EnemyType.LShield;
                break;
            case 12:
                _currentSpawnList.Add(EnemyType.RShield);
                forcedSpawn = EnemyType.RShield;
                break;
            case 13:
                _currentSpawnList.Add(EnemyType.Jumper);
                _currentSpawnList.Add(EnemyType.Bloat);
                break;
            case 14:
                _currentSpawnList.Add(EnemyType.Flatten);
                _currentSpawnList.Add(EnemyType.BabyForesight);
                break;
            case 15:
                _currentSpawnList.Add(EnemyType.LShield);
                _currentSpawnList.Add(EnemyType.RShield);
                forcedSpawn = EnemyType.Brute;
                break;
            case 16:
                _currentSpawnList.Add(EnemyType.Jumper);
                _currentSpawnList.Add(EnemyType.Bloat);
                break;
            case 17:
                _currentSpawnList.Add(EnemyType.Flatten);
                _currentSpawnList.Add(EnemyType.BabyForesight);
                break;
            case 18:
                _currentSpawnList.Add(EnemyType.LShield);
                _currentSpawnList.Add(EnemyType.RShield);
                break;
            case 19:
                _currentSpawnList.Add(EnemyType.Brute);
                _currentSpawnList.Add(EnemyType.Brute);
                break;
            case 20:
                _currentSpawnList.Add(EnemyType.Jumper);
                _currentSpawnList.Add(EnemyType.Jumper);
                _currentSpawnList.Add(EnemyType.Jumper);
                forcedSpawn = EnemyType.Brute;
                break;
            case 25:
                break;
            case 30:
                break;
            default:
                break;
        }
    }
}