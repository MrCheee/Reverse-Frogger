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

    // Start is called before the first frame update
    void Start()
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
            VehicleType.FastCar,
            VehicleType.FastCar,
            VehicleType.Motorbike,
            VehicleType.Motorbike,
            VehicleType.Truck,
            VehicleType.Truck,
            VehicleType.Bus
        };
    }

    public void SpawnXVehiclesAtRandom(int number)
    {
        List<int> reservedSpawnY = new List<int>() { dividerY };
        CheckOccupiedLanes(reservedSpawnY);

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
        for (int i = spawnYMin; i < spawnYMax+1; i++)
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
}
