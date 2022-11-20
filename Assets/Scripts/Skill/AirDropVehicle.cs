using System.Linq;
using UnityEngine;

public class AirDropVehicle : ISkill
{
    public Unit TargetUnit { get; set; }
    public GridCoord TargetGrid { get; set; }

    public AirDropVehicle(Unit target)
    {
        TargetUnit = target;
    }

    public void Execute()
    {
        if (TargetGrid.y < FieldGrid.DividerY)
        {
            TargetUnit.GetComponent<Vehicle>().ReverseMotion();
        }
        else
        {
            foreach (SpriteRenderer vehSprite in TargetUnit.GetComponentsInChildren<SpriteRenderer>())
            {
                vehSprite.flipY = true;
            }
            TargetUnit.transform.Rotate(Vector3.up, 180f);
        }

        TargetUnit.gameObject.SetActive(true);
        TargetUnit.GetComponent<Vehicle>().AddToFieldGridPosition(TargetGrid);
        TargetUnit.gameObject.transform.position = FieldGrid.GetGrid(TargetGrid).GetGridCentrePoint() + new Vector3(0, 55, 55);
        TargetUnit.GetComponent<Vehicle>().SimulateAirdrop();
    }

    public void UpdateGridCoordAction(GridCoord coord)
    {
        TargetGrid = coord;
    }
}