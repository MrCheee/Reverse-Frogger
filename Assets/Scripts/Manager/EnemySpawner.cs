using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] enemyPrefabs;

    List<EnemyType> _currentSpawnList;
    int spawnY = FieldGrid.GetFieldBuffer();
    int spawnXMin = FieldGrid.GetFieldBuffer() + 2;
    int spawnXMax = FieldGrid.GetMaxLength() - FieldGrid.GetFieldBuffer() - 3;

    private float stateInterval = 3.0f;
    //private float stateDelay = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        SetSpawnList();

        //StartCoroutine("SpawnOneSprinter");
        //StartCoroutine("SpawnTwoSoldiers");
        //StartCoroutine("SpawnEnemiesAtRandom");
    }

    void SetSpawnList()
    {
        // Spawn enemy types randomly based on proportion
        // Spawn enemy types in fixed sequence

        _currentSpawnList = new List<EnemyType>() { EnemyType.Soldier, EnemyType.Sprinter, EnemyType.Skater };
    }

    IEnumerator SpawnTwoSoldiers()
    {
        for (int i = 0; i < 2; i++)
        {
            GameObject unit = Instantiate(enemyPrefabs[0], Vector3.zero, enemyPrefabs[0].transform.rotation);
            unit.GetComponent<Enemy>().SetCurrentGridPosition(new GridCoord(0, 0));
            yield return new WaitForSeconds(stateInterval);
        }
    }

    IEnumerator SpawnOneSprinter()
    {
        int spawnX = 10;
        GridCoord spawnGrid = new GridCoord(spawnX, 0);
        Vector3 spawnPos = FieldGrid.GetSingleGrid(spawnGrid).GetGridCentrePoint();
        GameObject unit = Instantiate(enemyPrefabs[1], spawnPos, enemyPrefabs[1].transform.rotation);
        unit.GetComponent<Enemy>().SetCurrentGridPosition(spawnGrid);
        yield return new WaitForSeconds(stateInterval);
    }

    IEnumerator SpawnEnemiesAtRandom()
    {
        while (true)
        {
            int enemyIndex = Random.Range(0, enemyPrefabs.Length);
            int spawnX = Random.Range(2, 9);
            GridCoord spawnGrid = new GridCoord(spawnX, 0);
            Vector3 spawnPos = FieldGrid.GetSingleGrid(spawnGrid).GetGridCentrePoint();
            GameObject unit = Instantiate(enemyPrefabs[enemyIndex], spawnPos, enemyPrefabs[enemyIndex].transform.rotation);
            unit.GetComponent<Enemy>().AddToFieldGridPosition(spawnGrid);
            yield return new WaitForSeconds(stateInterval);
        }
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