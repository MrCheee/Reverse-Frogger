using UnityEngine;

public class AirDropVehicle : ISkill
{
    public GridCoord targetGrid { get; set; }
    public Unit unit { get; set; }

    public AirDropVehicle(Unit target)
    {
        unit = target;
    }

    public void Execute()
    {
        if (targetGrid.y < FieldGrid.GetDividerLaneNum())
        {
            unit.GetComponent<Vehicle>().ReverseMotion();
        }
        else
        {
            foreach (SpriteRenderer vehSprite in unit.GetComponentsInChildren<SpriteRenderer>())
            {
                vehSprite.flipY = true;
            }
            unit.transform.Rotate(Vector3.up, 180f);
        }
        unit.gameObject.transform.position = FieldGrid.GetSingleGrid(targetGrid).GetGridCentrePoint();
        unit.GetComponent<Vehicle>().AddToFieldGridPosition(targetGrid);
        unit.gameObject.SetActive(true);
    }

    public void UpdateGridCoordAction(GridCoord coord)
    {
        targetGrid = coord;
    }
}