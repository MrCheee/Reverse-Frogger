using System.Collections.Generic;
using UnityEngine;

public class EndlessEnemySpawner : MonoBehaviour, IEnemySpawner
{
    [SerializeField] private GameObject[] enemyPrefabs;

    const int EnemyBufferX = 2;  // Number of grids to buffer enemy spawn from the horizontal edges of the map
    int spawnY = FieldGrid.MaxPlayFieldY;
    int spawnXMin = FieldGrid.MinPlayFieldX + EnemyBufferX;
    int spawnXMax = FieldGrid.MaxPlayFieldX - EnemyBufferX;
    int level = 0;
    int spawnCount = 0;

    List<EnemyType> _currentSpawnList;
    Queue<EnemyType> forcedSpawn = new Queue<EnemyType>();

    public void SetDifficulty(string difficultyLevel)
    {
        forcedSpawn.Enqueue(EnemyType.Soldier);
        level = 1;
        if (difficultyLevel == "Expert")
        {
            _currentSpawnList = new List<EnemyType>()
            {
                EnemyType.Brute
            };
            spawnCount = 3;
        }
        else if (difficultyLevel == "Advanced")
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
            if (forcedSpawn.Count > 0)
            {
                enemyIndex = forcedSpawn.Dequeue();
            }
            else
            {
                enemyIndex = _currentSpawnList[Random.Range(0, _currentSpawnList.Count)];
            }
            int spawnX = RandomNonRepeating(reservedSpawnX, spawnXMin, spawnXMax);
            reservedSpawnX.Add(spawnX);
            GridCoord spawnGrid = new GridCoord(spawnX, spawnY);
            Vector3 spawnPos = FieldGrid.GetGrid(spawnGrid).GetGridCentrePoint();
            GameObject enemy = Instantiate(enemyPrefabs[(int)enemyIndex], spawnPos, enemyPrefabs[(int)enemyIndex].transform.rotation);
            enemy.GetComponent<Enemy>().AddToFieldGridPosition(spawnGrid);
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
                forcedSpawn.Enqueue(EnemyType.Skater);
                break;
            case 3:
                _currentSpawnList.Add(EnemyType.Sprinter);
                forcedSpawn.Enqueue(EnemyType.Sprinter);
                break;
            case 4:
                _currentSpawnList.Add(EnemyType.Charger);
                forcedSpawn.Enqueue(EnemyType.Charger);
                break;
            case 5:
                _currentSpawnList.Add(EnemyType.Brute);
                forcedSpawn.Enqueue(EnemyType.Brute);
                break;
            case 6:
                _currentSpawnList.Add(EnemyType.Vaulter);
                forcedSpawn.Enqueue(EnemyType.Vaulter);
                break;
            case 7:
                _currentSpawnList.Add(EnemyType.Bloat);
                forcedSpawn.Enqueue(EnemyType.Bloat);
                break;
            case 8:
                _currentSpawnList.Add(EnemyType.Jumper);
                forcedSpawn.Enqueue(EnemyType.Jumper);
                break;
            case 9:
                _currentSpawnList.Add(EnemyType.Flatten);
                forcedSpawn.Enqueue(EnemyType.Flatten);
                break;
            case 10:
                _currentSpawnList.Add(EnemyType.BabyForesight);
                _currentSpawnList.Add(EnemyType.Brute);
                forcedSpawn.Enqueue(EnemyType.BabyForesight);
                break;
            case 11:
                _currentSpawnList.Add(EnemyType.LShield);
                forcedSpawn.Enqueue(EnemyType.LShield);
                break;
            case 12:
                _currentSpawnList.Add(EnemyType.RShield);
                forcedSpawn.Enqueue(EnemyType.RShield);
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
                forcedSpawn.Enqueue(EnemyType.Brute);
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
                _currentSpawnList.Add(EnemyType.Jumper);
                _currentSpawnList.Add(EnemyType.Jumper);
                _currentSpawnList.Add(EnemyType.Jumper);
                forcedSpawn.Enqueue(EnemyType.Jumper);
                forcedSpawn.Enqueue(EnemyType.Jumper);
                break;
            case 20:
                _currentSpawnList.Add(EnemyType.Brute);
                _currentSpawnList.Add(EnemyType.Brute);
                forcedSpawn.Enqueue(EnemyType.Brute);
                spawnCount += 1;
                break;
            case 25:
                _currentSpawnList.Add(EnemyType.BabyForesight);
                _currentSpawnList.Add(EnemyType.BabyForesight);
                _currentSpawnList.Add(EnemyType.BabyForesight);
                forcedSpawn.Enqueue(EnemyType.BabyForesight);
                forcedSpawn.Enqueue(EnemyType.BabyForesight);
                forcedSpawn.Enqueue(EnemyType.Brute);
                break;
            case 30:
                spawnCount += 1;
                break;
            default:
                break;
        }
    }
}