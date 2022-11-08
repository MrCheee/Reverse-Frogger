using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SingleGrid : ISingleGrid
{
    GridCoord _gridCoord;
    Vector3 _centrePoint;
    float direction = -1f;
    Dictionary<int, GameObject> _objectsID = new Dictionary<int, GameObject>();
    Dictionary<string, float> enemiesOnTopPositioning = new Dictionary<string, float>()
    {
        { "Car", 0.3f },
        { "Fast Car", 0.2f },
        { "Truck", 0.5f },
        { "Bus", 0.5f }
    };

    public SingleGrid(int x, int y, Vector3 centrePoint)
    {
        _gridCoord.x = x;
        _gridCoord.y = y;
        _centrePoint = centrePoint;
    }

    // To position multiple game objects that are in the same grid so they do not overlap
    // Additional rules for sorting which enemy will be given priority to be on the left/right
    // LShield has top priority on left, followed by brute
    // RShield has top priority on right, followed by brute
    public void RepositionObjects()
    {
        List<Unit> allEnemies = GetListOfUnitsWithGameObjectTag("Enemy");
        List<Unit> enemiesOnTop = new List<Unit>();

        if (_objectsID.Values.Select(x => x.tag).Contains("Vehicle"))
        {
            enemiesOnTop = allEnemies.Where(x => x.yAdjustment > 0).ToList();
            allEnemies = allEnemies.Where(x => x.yAdjustment <= 0).ToList();
        }

        // For bottom enemies
        if (allEnemies.Count > 1)
        {
            allEnemies = RearrangeEnemies(allEnemies);
        }
        switch (allEnemies.Count)
        {
            case 2:
                allEnemies[0].IssueCommand(new MoveWithinGridCommand(_centrePoint + Vector3.left));
                allEnemies[1].IssueCommand(new MoveWithinGridCommand(_centrePoint + Vector3.right));
                break;
            case 3:
                allEnemies[0].IssueCommand(new MoveWithinGridCommand(_centrePoint + Vector3.left + Vector3.forward));
                allEnemies[1].IssueCommand(new MoveWithinGridCommand(_centrePoint + Vector3.back));
                allEnemies[2].IssueCommand(new MoveWithinGridCommand(_centrePoint + Vector3.right + Vector3.forward));
                break;
            case 4:
                allEnemies[0].IssueCommand(new MoveWithinGridCommand(_centrePoint + Vector3.left + Vector3.forward));
                allEnemies[1].IssueCommand(new MoveWithinGridCommand(_centrePoint + Vector3.left + Vector3.back));
                allEnemies[2].IssueCommand(new MoveWithinGridCommand(_centrePoint + Vector3.right + Vector3.forward));
                allEnemies[3].IssueCommand(new MoveWithinGridCommand(_centrePoint + Vector3.right + Vector3.back));
                break;
            case 5:
                allEnemies[0].IssueCommand(new MoveWithinGridCommand(_centrePoint + Vector3.left + Vector3.forward));
                allEnemies[1].IssueCommand(new MoveWithinGridCommand(_centrePoint + Vector3.left + Vector3.back));
                allEnemies[2].IssueCommand(new MoveWithinGridCommand(_centrePoint + Vector3.right + Vector3.forward));
                allEnemies[3].IssueCommand(new MoveWithinGridCommand(_centrePoint + Vector3.right + Vector3.back));
                allEnemies[4].IssueCommand(new MoveWithinGridCommand(_centrePoint));
                break;
            default:
                // No repositioning if only 1 Unit or none is in the grid
                break;
        }

        // For top enemies
        if (enemiesOnTop.Count > 1)
        {
            int totalTopEnemies = enemiesOnTop.Count;
            string vehicleName = _objectsID.Values.Where(x => x.tag == "Vehicle").First().GetComponent<Unit>().GetName();
            float maxDeviationPos = enemiesOnTopPositioning[vehicleName];
            float distInterval = (maxDeviationPos * 2) / (totalTopEnemies - 1);

            for (int i = 0; i < enemiesOnTop.Count; i++)
            {
                enemiesOnTop[i].IssueCommand(new MoveWithinGridCommand(new Vector3(_centrePoint.x - maxDeviationPos + distInterval * i, 0, _centrePoint.z - direction * 1.25f)));
            }
        }
    }

    private List<Unit> RearrangeEnemies(List<Unit> enemies)
    {
        bool leftLane = _gridCoord.y < FieldGrid.GetDividerLaneNum() ? true : false;
        List<Tuple<int, Unit>> rearrangedEnemies = new List<Tuple<int, Unit>>();
        for (int i = 0; i < enemies.Count; i++)
        {
            int sortIndex = 2;
            string enemyName = enemies[i].GetTag();
            if (enemyName == "LShield")
            {
                sortIndex = 0;
            }
            else if (enemyName == "RShield")
            {
                sortIndex = 4;
            }
            else if (enemyName == "Brute")
            {
                if (leftLane)
                {
                    sortIndex = 3;
                }
                else
                {
                    sortIndex = 1;
                }
            }
            rearrangedEnemies.Add(new Tuple<int, Unit>(sortIndex, enemies[i]));
        }
        rearrangedEnemies.Sort((x, y) => x.Item1.CompareTo(y.Item1));
        List<Unit> finalArrangedEnemies = rearrangedEnemies.Select(x => x.Item2).ToList();
        return finalArrangedEnemies;
    }

    public void AddObject(GameObject gameobj)
    {
        _objectsID.Add(gameobj.GetInstanceID(), gameobj);
        FieldGrid.AddGridToReposition(_gridCoord);
    }

    public void RemoveObject(int gameobjID)
    {
        _objectsID.Remove(gameobjID);
        FieldGrid.AddGridToReposition(_gridCoord);
    }

    public Vector3 GetGridCentrePoint()
    {
        return _centrePoint;
    }

    public Vector3 GetCornerPoint(int right, int top)
    {
        return new Vector3((float)(_centrePoint.x + right * 1.5), 0, (float)(_centrePoint.z + top * 1.5));
    }

    public Vector3 GetInBetweenPoint(int front, int top)
    {
        return new Vector3((float)(_centrePoint.x + front * 1.75), 0, (float)(_centrePoint.z + top * 2));
    }

    public int GetUnitCount()
    {
        return _objectsID.Where(x => x.Value.CompareTag("Unit")).Count();
    }

    public int GetObjectCount()
    {
        return _objectsID.Count;
    }

    public List<string> GetListOfUnitsGameObjectTag()
    {
        return _objectsID.Values.Select(x => x.tag).ToList();
    }

    public List<string> GetListOfUnitsTag()
    {
        return _objectsID.Values.Select(x => x.GetComponent<Unit>().GetTag()).ToList();
    }

    public List<Unit> GetListOfUnitsWithGameObjectTag(string tag)
    {
        return _objectsID.Values.Where(x => x.CompareTag(tag)).Select(x => x.GetComponent<Unit>()).ToList();
    }

    public List<string> GetListOfUnitsName()
    {
        return _objectsID.Values.Select(x => x.GetComponent<Unit>().GetName()).ToList();
    }

    public bool IsUnitTagInGrid(string tag)
    {
        return _objectsID.Values.Select(x => x.GetComponent<Unit>().GetTag()).Contains(tag);
    }

    public Unit GetUnitWithTag(string tag)
    {
        return _objectsID.Values.Where(x => x.GetComponent<Unit>().GetTag() == tag).First().GetComponent<Unit>();
    }

    public List<Unit> GetListOfUnitsWithTag(string tag)
    {
        return _objectsID.Values.Where(x => x.GetComponent<Unit>().GetTag() == tag).Select(x => x.GetComponent<Unit>()).ToList();
    }

    public GridCoord GetGridCoord()
    {
        return _gridCoord;
    }
}