using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class VehicleSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] vehPrefabs;

    List<VehicleType> _currentSpawnList;
    int numOfLanes = FieldGrid.GetNumberOfLanes() * 2;
    int spawnXLeft = FieldGrid.GetFieldBuffer() - 1;
    int spawnXRight = FieldGrid.GetMaxLength() - FieldGrid.GetFieldBuffer();
    int spawnYMin = FieldGrid.GetFieldBuffer() + 1;
    int spawnYMax = FieldGrid.GetFieldBuffer() + FieldGrid.GetNumberOfLanes() * 2 + 1;
    int dividerY = FieldGrid.GetFieldBuffer() + 1 + FieldGrid.GetNumberOfLanes();

    private float stateInterval = 3.0f;
    private float stateDelay = 1.5f;

    // Start is called before the first frame update
    void Start()
    {
        SetSpawnList();

        //StartCoroutine("SpawnOneCar");
        //StartCoroutine("SpawnVehiclesAtRandom");
    }

    void SetSpawnList()
    {
        _currentSpawnList = new List<VehicleType>() { VehicleType.Car, VehicleType.SpeedyCar };
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

            if (spawnY < dividerY) veh.GetComponent<Vehicle>().reverseMotion();

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

        if (spawnY < dividerY) veh.GetComponent<Vehicle>().reverseMotion();

        yield return new WaitForSeconds(stateInterval);
    }

    public void SpawnXVehiclesAtRandom(int number)
    {
        List<int> reservedSpawnY = new List<int>() { dividerY };
        if (number > numOfLanes)
        {
            Debug.Log($"Requested too many vehicles to spawn, there is limited space of {numOfLanes}!!");
            return;
        }

        for (int i = 0; i < number; i++)
        {
            int spawnIndex = Random.Range(0, _currentSpawnList.Count);
            VehicleType vehIndex = _currentSpawnList[spawnIndex];
            int spawnY = RandomNonRepeating(reservedSpawnY, spawnYMin, spawnYMax);
            reservedSpawnY.Add(spawnY);
            int spawnX = spawnY < dividerY ? spawnXRight : spawnXLeft;
            GridCoord spawnGrid = new GridCoord(spawnX, spawnY);
            Vector3 spawnPos = FieldGrid.GetSingleGrid(spawnGrid).GetGridCentrePoint();
            GameObject veh = Instantiate(vehPrefabs[(int)vehIndex], spawnPos, vehPrefabs[(int)vehIndex].transform.rotation);
            veh.GetComponent<Vehicle>().AddToFieldGridPosition(spawnGrid);
            if (spawnY < dividerY) veh.GetComponent<Vehicle>().reverseMotion();
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
