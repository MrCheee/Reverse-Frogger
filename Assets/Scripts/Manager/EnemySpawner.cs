using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] enemyPrefabs;

    List<EnemyType> _currentSpawnList;
    int spawnY = FieldGrid.GetFieldBuffer();
    int spawnXMin = FieldGrid.GetFieldBuffer() + 2;
    int spawnXMax = FieldGrid.GetMaxLength() - FieldGrid.GetFieldBuffer() - 2;

    // Start is called before the first frame update
    void Start()
    {
        SetSpawnList();
    }

    void SetSpawnList()
    {
        // Spawn enemy types randomly based on proportion
        // Spawn enemy types in fixed sequence

        //_currentSpawnList = new List<EnemyType>() { EnemyType.Soldier, EnemyType.Sprinter, EnemyType.Skater };

        _currentSpawnList = new List<EnemyType>()
        {
            EnemyType.Flatten
        };
    }

    public void SpawnXEnemiesAtRandom(int number)
    {
        List<int> reservedSpawnX = new List<int>();
        if (number > (spawnXMax - spawnXMin))
        {
            Debug.Log($"Requested too many enemies to spawn, there is limited space of {spawnXMax - spawnXMin}!!");
            return;
        }

        for (int i = 0; i < number; i++)
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