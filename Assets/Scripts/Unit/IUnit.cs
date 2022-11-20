using UnityEngine;

interface IUnit
{
    GridCoord GetCurrentHeadGridPosition();
    void UpdateGridMovement(GridCoord position);
    void BeginPreTurn();
    void BeginTurn();
    void BeginPostTurn();
    void Move(Vector3 direction);
    void Rotate(Vector3 rotateAxis, float rotateAmt);
    void TakeDamage(int damageReceived);
    void IssueCommand(Command cmd);
    void DisableUnit(int disableTime);
}
