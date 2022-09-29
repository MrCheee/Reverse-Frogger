public class AirDropVeh : Skill
{
    public AirDropVeh(Unit target) : base(target) { }

    public GridCoord spawnGrid { get; set; }

    public override void Execute()
    {
        unit.gameObject.transform.position = FieldGrid.GetSingleGrid(spawnGrid).GetGridCentrePoint();
    }

    public override void UpdateGridCoordAction(GridCoord coord)
    {
        spawnGrid = coord;
    }
}