using UnityEngine;

public class PlayerSkillVehicleManager : MonoBehaviour, IPlayerSkillVehicleManager
{
    [SerializeField] private GameObject[] vehPrefabs;
    private GameObject[] callInVehicles;
    private Vector3 playerSkillVehSpawnPos;

    void Awake()
    {
        GeneratePlayerSkillVehicles();
    }

    public void GeneratePlayerSkillVehicles()
    {
        GridCoord spawnGrid = new GridCoord(FieldGrid.FieldLength / 2, FieldGrid.FieldHeight / 2);
        Vector3 spawnPos = FieldGrid.GetGrid(spawnGrid).GetGridCentrePoint();
        spawnPos.y = -100;

        callInVehicles = new GameObject[vehPrefabs.Length];
        playerSkillVehSpawnPos = spawnPos;

        for (int i = 0; i < vehPrefabs.Length; i++)
        {
            callInVehicles[i] = Instantiate(vehPrefabs[i], playerSkillVehSpawnPos, vehPrefabs[i].transform.rotation);
            callInVehicles[i].SetActive(false);
        }
    }

    public GameObject GetPlayerSkillVehicle(int index)
    {
        return callInVehicles[index];
    }

    public void ReplacePlayerSkillVehicles()
    {
        for (int i = 0; i < vehPrefabs.Length; i++)
        {
            if (callInVehicles[i].gameObject.activeInHierarchy)
            {
                callInVehicles[i] = Instantiate(vehPrefabs[i], playerSkillVehSpawnPos, vehPrefabs[i].transform.rotation);
                callInVehicles[i].SetActive(false);
            }
        }
    }
}