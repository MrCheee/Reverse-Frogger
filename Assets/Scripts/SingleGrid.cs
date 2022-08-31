using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SingleGrid : ISingleGrid
{
    int _x;
    int _y;
    Vector3 _centrePoint;
    Dictionary<int, GameObject> _objectsID = new Dictionary<int, GameObject>();

    public SingleGrid(int x, int y, Vector3 centrePoint)
    {
        _x = x;
        _y = y;
        _centrePoint = centrePoint;
    }

    // To position multiple game objects that are in the same grid so they do not overlap
    public void RepositionObjects()
    {
        List<Unit> allEnemies = GetListOfObjectTypes("Enemy");
        switch (allEnemies.Count)
        {
            case 2:
                allEnemies[0].IssueCommand(new MoveWithinGridCommand(_centrePoint + Vector3.left));
                allEnemies[1].IssueCommand(new MoveWithinGridCommand(_centrePoint + Vector3.right));
                break;
            case 3:
                allEnemies[0].IssueCommand(new MoveWithinGridCommand(_centrePoint + Vector3.left + Vector3.forward));
                allEnemies[1].IssueCommand(new MoveWithinGridCommand(_centrePoint + Vector3.right + Vector3.forward));
                allEnemies[2].IssueCommand(new MoveWithinGridCommand(_centrePoint + Vector3.back));
                break;
            case 4:
                allEnemies[0].IssueCommand(new MoveWithinGridCommand(_centrePoint + Vector3.left + Vector3.forward));
                allEnemies[1].IssueCommand(new MoveWithinGridCommand(_centrePoint + Vector3.right + Vector3.forward));
                allEnemies[2].IssueCommand(new MoveWithinGridCommand(_centrePoint + Vector3.left + Vector3.back));
                allEnemies[3].IssueCommand(new MoveWithinGridCommand(_centrePoint + Vector3.right + Vector3.back));
                break;
            default:
                // No repositioning if only 1 Unit or none is in the grid
                break;
        }
    }

    public void AddObject(GameObject gameobj)
    {
        _objectsID.Add(gameobj.GetInstanceID(), gameobj);
        RepositionObjects();
    }

    public void RemoveObject(int gameobjID)
    {
        _objectsID.Remove(gameobjID);
    }

    public Vector3 GetGridCentrePoint()
    {
        return _centrePoint;
    }

    public int GetUnitCount()
    {
        return _objectsID.Where(x => x.Value.CompareTag("Unit")).Count();
    }

    public int GetObjectCount()
    {
        return _objectsID.Count;
    }

    public List<string> GetUnitsTag()
    {
        return _objectsID.Values.Select(x => x.tag).ToList();
    }

    List<Unit> GetListOfObjectTypes(string tag)
    {
        return _objectsID.Values.Where(x => x.CompareTag(tag)).Select(x => x.GetComponent<Unit>()).ToList();
    }
}