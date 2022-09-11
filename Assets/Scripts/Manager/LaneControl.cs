using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaneControl : MonoBehaviour
{
    GameObject[] speedIndicators = new GameObject[3];
    private int laneSpeed = 1;
    private int speedToggle = 0;
    private int minSpeed = 1;
    private int maxSpeed = 3;

    public int LaneSpeed { get => laneSpeed; private set => laneSpeed = value; }

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < speedIndicators.Length; i++)
        {
            speedIndicators[i] = gameObject.transform.GetChild(i).gameObject;
        }

        InitSpeedIndicator();
    }

    void InitSpeedIndicator()
    {
        speedIndicators[0].SetActive(true);
    }

    public void AddSpeed(bool toggledOn)
    {
        if (toggledOn)
        {
            speedToggle = 1;
            UpdateSpeedIndicators();
        }
    }

    public void LowerSpeed(bool toggledOn)
    {
        if (toggledOn)
        {
            speedToggle = -1;
            UpdateSpeedIndicators();
        }
    }

    public void SameSpeed(bool toggledOn)
    {
        if (toggledOn)
        {
            speedToggle = 0;
            UpdateSpeedIndicators();
        }
    }

    void ToggleSpeedUp()
    {
        LaneSpeed = Mathf.Min(maxSpeed, LaneSpeed + 1);
    }

    void ToggleSpeedDown()
    {
        LaneSpeed = Mathf.Max(minSpeed, LaneSpeed - 1);
    }

    void ToggleSpeedSame()
    {
        LaneSpeed = Mathf.Max(minSpeed, LaneSpeed - 1);
    }

    void UpdateSpeedIndicators()
    {
        int tmpSpeed = Mathf.Max(minSpeed, Mathf.Min(maxSpeed, LaneSpeed + speedToggle));
        Debug.Log($"Temp Speed: {tmpSpeed}");
        for (int i = 0; i < speedIndicators.Length; i++)
        {
            if (i < tmpSpeed)
            {
                speedIndicators[i].SetActive(true);
            }
            else
            {
                speedIndicators[i].SetActive(false);
            }
        }
    }

    public void FinaliseSpeed()
    {
        LaneSpeed = Mathf.Max(minSpeed, Mathf.Min(maxSpeed, LaneSpeed + speedToggle));
        speedToggle = 0;
    }
}