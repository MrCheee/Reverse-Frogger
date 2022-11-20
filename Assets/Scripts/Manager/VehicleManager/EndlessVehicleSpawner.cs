using System.Collections.Generic;
using UnityEngine;

public class EndlessVehicleSpawner : MonoBehaviour, IVehicleSpawner
{
    [SerializeField] private GameObject[] vehPrefabs;

    List<VehicleType> _currentSpawnList;
    float doubleSpawnProbability;
    int failedAttempts;
    int level;
    int initCount;
    int numOfLanes;
    int spawnXLeft;
    int spawnXRight;
    int spawnYMin;
    int spawnYMax;
    int dividerY;

    // Start is called before the first frame update
    void Start()
    {
        doubleSpawnProbability = 0.2f;  // Using pseudo-random distribution = roughly 40% on average
        failedAttempts = 1;
        level = 1;
        initCount = 5;
        numOfLanes = FieldGrid.NumOfLanes * 2;
        spawnXLeft = FieldGrid.FieldBuffer - 1;
        spawnXRight = FieldGrid.FieldLength - FieldGrid.FieldBuffer;
        spawnYMin = FieldGrid.FieldBuffer + 1;
        spawnYMax = FieldGrid.FieldBuffer + FieldGrid.NumOfLanes * 2 + 1;
        dividerY = FieldGrid.DividerY;
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
            VehicleType.Car,
            VehicleType.FastCar,
            VehicleType.FastCar,
            VehicleType.FastCar,
            VehicleType.FastCar,
            VehicleType.Truck,
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
            Vector3 spawnPos = FieldGrid.GetGrid(spawnGrid).GetGridCentrePoint();
            Quaternion spawnRotation = spawnY < dividerY ? vehPrefabs[(int)vehIndex].transform.rotation : vehPrefabs[(int)vehIndex].transform.rotation * Quaternion.Euler(0f, 180f, 0f);

            GameObject veh = Instantiate(vehPrefabs[(int)vehIndex], spawnPos, spawnRotation);
            veh.GetComponent<Vehicle>().AddToFieldGridPosition(spawnGrid);
            if (spawnY < dividerY)
            {
                veh.GetComponent<Vehicle>().ReverseMotion();
            }
            else
            {
                foreach (SpriteRenderer vehSprite in veh.GetComponentsInChildren<SpriteRenderer>())
                {
                    vehSprite.flipY = true;
                }
            }
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
            Vector3 spawnPos = FieldGrid.GetGrid(spawnGrid).GetGridCentrePoint();
            Quaternion spawnRotation = spawnY < dividerY ? vehPrefabs[(int)vehIndex].transform.rotation : vehPrefabs[(int)vehIndex].transform.rotation * Quaternion.Euler(0f, 180f, 0f);

            GameObject veh = Instantiate(vehPrefabs[(int)vehIndex], spawnPos, spawnRotation);
            veh.GetComponent<Vehicle>().AddToFieldGridPosition(spawnGrid);
            if (spawnY < dividerY)
            {
                veh.GetComponent<Vehicle>().ReverseMotion();
            }
            else
            {
                foreach (SpriteRenderer vehSprite in veh.GetComponentsInChildren<SpriteRenderer>())
                {
                    vehSprite.flipY = true;
                }
            }
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
                if (FieldGrid.GetGrid(spawnXRight, i).GetListOfUnitsGameObjectTag().Contains("Vehicle"))
                {
                    reservedSpawnY.Add(i);
                }
            }
            else if (i > dividerY)
            {
                if (FieldGrid.GetGrid(spawnXLeft, i).GetListOfUnitsGameObjectTag().Contains("Vehicle"))
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