using System.Collections.Generic;
using UnityEngine;

public class LaneManager : MonoBehaviour
{
    [SerializeField] private GameObject[] laneObjects;
    private List<LaneControl> lanes = new List<LaneControl>();

    private void Start()
    {
        foreach(GameObject laneObj in laneObjects)
        {
            lanes.Add(laneObj.GetComponent<LaneControl>());
        }
    }

    public int GetLaneSpeed(int gridYNum)
    {
        int laneIndex = gridYNum - FieldGrid.FieldBuffer - 1; // -1 for the sidewalk
        if (laneIndex >= FieldGrid.NumOfLanes)
        {
            laneIndex -= 1;
        }
        if (laneIndex >= lanes.Count)
        {
            // To Throw Exception
            return 0;
        }
        return lanes[laneIndex].LaneSpeed;
    }
}