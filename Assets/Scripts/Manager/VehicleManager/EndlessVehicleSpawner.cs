using System.Collections.Generic;
using UnityEngine;

public class EndlessVehicleSpawner : MonoBehaviour, IVehicleSpawner
{
    [SerializeField] private GameObject[] vehPrefabs;

    List<VehicleType> _currentSpawnList;
    float doubleSpawnProbability = 0.2f;  // Using pseudo-random distribution = roughly 40% on average
    int failedAttempts = 1;
    int level = 1;
    int initCount = 5;

    int numOfLanes = FieldGrid.GetNumberOfLanes() * 2;
    int spawnXLeft = FieldGrid.GetFieldBuffer() - 1;
    int spawnXRight = FieldGrid.GetMaxLength() - FieldGrid.GetFieldBuffer();
    int spawnYMin = FieldGrid.GetFieldBuffer() + 1;
    int spawnYMax = FieldGrid.GetFieldBuffer() + FieldGrid.GetNumberOfLanes() * 2 + 1;
    int dividerY = FieldGrid.GetFieldBuffer() + 1 + FieldGrid.GetNumberOfLanes();

    // Start is called before the first frame update
    void Awake()
    {
        SetSpawnList();
    }

    void SetSpawnList()
    {
        _currentSpawnList = new List<VehicleType>()
        {
            VehicleType.Car,
            VehicleType.Car,
            VehicleType.Car,
            VehicleType.Car,
            VehicleType.FastCar,
            VehicleType.FastCar,
            VehicleType.Motorbike,
            VehicleType.Truck,
            VehicleType.Bus
        };
    }

    public void PopulateInitialVehicles()
    {
        List<int> reservedSpawnY = new List<int>() { dividerY };
        for (int i = 0; i < initCount; i++)
        {
            VehicleType vehIndex = VehicleType.Car;

            int spawnY = RandomNonRepeating(reservedSpawnY, spawnYMin, spawnYMax);
            reservedSpawnY.Add(spawnY);
            int spawnX = Random.Range(spawnXLeft + 1, spawnXRight - 1);

            GridCoord spawnGrid = new GridCoord(spawnX, spawnY);
            Vector3 spawnPos = FieldGrid.GetSingleGrid(spawnGrid).GetGridCentrePoint();
            Quaternion spawnRotation = spawnY < dividerY ? vehPrefabs[(int)vehIndex].transform.rotation : vehPrefabs[(int)vehIndex].transform.rotation * Quaternion.Euler(0f, 180f, 0f);

            GameObject veh = Instantiate(vehPrefabs[(int)vehIndex], spawnPos, spawnRotation);

            veh.GetComponent<Vehicle>().AddToFieldGridPosition(spawnGrid);
            if (spawnY < dividerY) veh.GetComponent<Vehicle>().ReverseMotion();
        }
    }

    public void SpawnVehicles()
    {
        List<int> reservedSpawnY = new List<int>() { dividerY };
        CheckOccupiedLanes(reservedSpawnY);

        int vehicleCount = CheckNumberOfVehiclesToSpawn();
        vehicleCount = Mathf.Min(vehicleCount, numOfLanes - reservedSpawnY.Count);  // Limit vehicle spawn by number of available spawn lanes

        for (int i = 0; i < vehicleCount; i++)
        {
            int spawnIndex = Random.Range(0, _currentSpawnList.Count);
            VehicleType vehIndex = _currentSpawnList[spawnIndex];

            int spawnY = RandomNonRepeating(reservedSpawnY, spawnYMin, spawnYMax);
            reservedSpawnY.Add(spawnY);
            int spawnX = spawnY < dividerY ? spawnXRight : spawnXLeft;

            GridCoord spawnGrid = new GridCoord(spawnX, spawnY);
            Vector3 spawnPos = FieldGrid.GetSingleGrid(spawnGrid).GetGridCentrePoint();
            Quaternion spawnRotation = spawnY < dividerY ? vehPrefabs[(int)vehIndex].transform.rotation : vehPrefabs[(int)vehIndex].transform.rotation * Quaternion.Euler(0f, 180f, 0f);

            GameObject veh = Instantiate(vehPrefabs[(int)vehIndex], spawnPos, spawnRotation);

            veh.GetComponent<Vehicle>().AddToFieldGridPosition(spawnGrid);
            if (spawnY < dividerY) veh.GetComponent<Vehicle>().ReverseMotion();
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

    void CheckOccupiedLanes(List<int> reservedSpawnY)
    {
        for (int i = spawnYMin; i < spawnYMax + 1; i++)
        {
            if (i < dividerY)
            {
                if (FieldGrid.GetSingleGrid(spawnXRight, i).GetListOfUnitsGameObjectTag().Contains("Vehicle"))
                {
                    reservedSpawnY.Add(i);
                }
            }
            else if (i > dividerY)
            {
                if (FieldGrid.GetSingleGrid(spawnXLeft, i).GetListOfUnitsGameObjectTag().Contains("Vehicle"))
                {
                    reservedSpawnY.Add(i);
                }
            }
        }
    }

    int CheckNumberOfVehiclesToSpawn()
    {
        float currentProbability = doubleSpawnProbability * failedAttempts;
        if (currentProbability >= 1) // Pseudo-random has hit guaranteed double spawn threshold
        {
            failedAttempts = 1;
            return 2;
        }
        else
        {
            float roll = Random.Range(0.0f, 1.0f);
            if (roll < currentProbability)   // Rolled double spawn, reset N to 1
            {
                failedAttempts = 1;
                return 2;
            }
            else     // Failed double spawn roll, increment N
            {
                failedAttempts += 1;
                return 1;
            }
        }

    }

    public void IncrementLevel()
    {
        level += 1;
    }
}