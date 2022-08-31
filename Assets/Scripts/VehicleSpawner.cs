using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] vehPrefabs;
    private float stateInterval = 3.0f;
    private float stateDelay = 1.5f;

    // Start is called before the first frame update
    void Start()
    {
        //StartCoroutine("SpawnOneCar");
        StartCoroutine("SpawnVehiclesAtRandom");
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator SpawnVehiclesAtRandom()
    {
        yield return new WaitForSeconds(stateDelay);
        while (true)
        {
            int vehIndex = Random.Range(0, vehPrefabs.Length);
            int spawnY = Random.Range(1, 7);
            GridCoord spawnGrid = (spawnY < 4) ? new GridCoord(10, spawnY) : new GridCoord(0, spawnY+1);
            Vector3 spawnPos = FieldGrid.GetSingleGrid(spawnGrid).GetGridCentrePoint();
            GameObject veh = Instantiate(vehPrefabs[vehIndex], spawnPos, vehPrefabs[vehIndex].transform.rotation);
            veh.GetComponent<Vehicle>().AddToFieldGridPosition(spawnGrid);

            if (spawnY < 4) veh.GetComponent<Vehicle>().reverseMotion();

            yield return new WaitForSeconds(stateInterval);
        }
    }

    IEnumerator SpawnOneCar()
    {
        yield return new WaitForSeconds(stateDelay);
        int spawnY = 2;
        int spawnX = 10;
        GridCoord spawnGrid = new GridCoord(spawnX, spawnY);
        Vector3 spawnPos = FieldGrid.GetSingleGrid(spawnGrid).GetGridCentrePoint();
        GameObject veh = Instantiate(vehPrefabs[0], spawnPos, vehPrefabs[0].transform.rotation);
        veh.GetComponent<Vehicle>().AddToFieldGridPosition(spawnGrid);

        if (spawnY < 4) veh.GetComponent<Vehicle>().reverseMotion();

        yield return new WaitForSeconds(stateInterval);
    }
}
