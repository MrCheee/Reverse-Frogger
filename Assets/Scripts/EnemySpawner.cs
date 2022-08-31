using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] enemyPrefabs;
    private float stateInterval = 3.0f;
    //private float stateDelay = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        //StartCoroutine("SpawnOneSprinter");
        //StartCoroutine("SpawnTwoSoldiers");
        StartCoroutine("SpawnEnemiesAtRandom");
    }

    // Update is called once per frame
    void Update()
    {
        
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
        while(true)
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
}
