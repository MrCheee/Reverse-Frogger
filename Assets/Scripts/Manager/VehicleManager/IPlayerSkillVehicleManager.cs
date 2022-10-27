using UnityEngine;

public interface IPlayerSkillVehicleManager
{
    void GeneratePlayerSkillVehicles();
    GameObject GetPlayerSkillVehicle(int index);
    void ReplacePlayerSkillVehicles();
}