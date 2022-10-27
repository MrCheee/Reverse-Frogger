using UnityEngine;

public interface IVehicleSpawner
{
    void PopulateInitialVehicles();
    void SpawnVehicles();
    void IncrementLevel();
}