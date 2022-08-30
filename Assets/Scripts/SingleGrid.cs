using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SingleGrid : ISingleGrid
{
    int _x;
    int _y;
    Vector3 _centrePoint;
    List<Unit> _units = new List<Unit>();

    public SingleGrid(int x, int y, Vector3 centrePoint)
    {
        _x = x;
        _y = y;
        _centrePoint = centrePoint;
    }

    // To position multiple game objects that are in the same grid so they do not overlap
    public void RepositionUnits()
    {
        int count = _units.Count;
        switch (count)
        {
            case 2:
                _units[0].IssueCommand(new MoveCommand(Vector3.left, _units[0].transform.position + Vector3.left));
                _units[1].IssueCommand(new MoveCommand(Vector3.right, _units[1].transform.position + Vector3.right));
                break;
            case 3:
                _units[0].IssueCommand(new MoveCommand(Vector3.forward, _units[0].transform.position + Vector3.forward));
                _units[1].IssueCommand(new MoveCommand(Vector3.forward, _units[1].transform.position + Vector3.forward));
                _units[2].IssueCommand(new MoveCommand(Vector3.back, _units[2].transform.position + Vector3.back));
                break;
            default:
                break;
        }
    }

    public void AddUnit(Unit gameobj)
    {
        _units.Add(gameobj);
        RepositionUnits();
    }

    public Vector3 GetGridPoint()
    {
        return _centrePoint;
    }

    public int GetUnitCount()
    {
        return _units.Count;
    }

    public string GetUnitsTag()
    {
        if (_units.Count > 0)
        {
            return _units[0].tag;
        }
        else
        {
            return "None";
        }
    }
}