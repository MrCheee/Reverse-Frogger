using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DevEnemySpawner : MonoBehaviour, IEnemySpawner
{
    [SerializeField] private GameObject[] enemyPrefabs;

    List<EnemyType> _currentSpawnList;
    int spawnY = FieldGrid.GetMaxHeight() - FieldGrid.GetFieldBuffer() - 1;
    int spawnXMin = FieldGrid.GetFieldBuffer() + 2;
    int spawnXMax = FieldGrid.GetMaxLength() - FieldGrid.GetFieldBuffer() - 2;
    int level = 2;

    // Start is called before the first frame update
    void Start()
    {
        _currentSpawnList = new List<EnemyType>()
        {
            EnemyType.Soldier, 
            EnemyType.Sprinter, 
            EnemyType.Skater,
            EnemyType.Brute,
            EnemyType.Bloat,
            EnemyType.BabyForesight,
            EnemyType.Charger,
            EnemyType.Flatten,
            EnemyType.Jumper,
            EnemyType.Vaulter,
            EnemyType.LShield,
            EnemyType.RShield
        };
    }

    public void SpawnEnemies()
    {
        List<int> reservedSpawnX = new List<int>();
        int spawnCount = Mathf.Min(level, spawnXMax - spawnXMin);

        for (int i = 0; i < spawnCount; i++)
        {
            int spawnIndex = Random.Range(0, _currentSpawnList.Count);
            EnemyType enemyIndex = _currentSpawnList[spawnIndex];
            int spawnX = RandomNonRepeating(reservedSpawnX, spawnXMin, spawnXMax);
            reservedSpawnX.Add(spawnX);
            GridCoord spawnGrid = new GridCoord(spawnX, spawnY);
            Vector3 spawnPos = FieldGrid.GetSingleGrid(spawnGrid).GetGridCentrePoint();
            GameObject enemy = Instantiate(enemyPrefabs[(int)enemyIndex], spawnPos, enemyPrefabs[(int)enemyIndex].transform.rotation);
            enemy.GetComponent<Enemy>().AddToFieldGridPosition(spawnGrid);
        }
    }

    public void SetDifficulty(string difficultyLevel)
    {
        return;
    }

    public void IncrementLevel()
    {
        return;
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
}