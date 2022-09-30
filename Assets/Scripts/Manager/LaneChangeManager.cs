using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LaneChangeManager : MonoBehaviour
{
    public Camera GameCamera;
    [SerializeField] private Canvas canvas;
    [SerializeField] private RectTransform laneChangeUI;
    [SerializeField] private Button laneChangeUpButton;
    [SerializeField] private Button laneChangeDownButton;
    [SerializeField] private RectTransform laneChangeUpButtonImg;
    [SerializeField] private RectTransform laneChangeDownButtonImg;
    private Color laneChangeOnColor;
    private Color laneChangeOffColor;
    private Color laneChangeBlockColor;
    private Skill currentLaneChangeSkillHolder;

    private void Start()
    {
        laneChangeOnColor = new Color(255, 182, 0, 0.5f);
        laneChangeOffColor = new Color(161, 161, 161, 0.5f);
        laneChangeBlockColor = new Color(255, 0, 0, 0.5f);
    }

    public void RegisterLaneChangeTarget(Skill currentSkill)
    {
        currentLaneChangeSkillHolder = currentSkill;

        // To make the lane change UI be centred onto the selected grid, even if user clicks at random spot on the vehicle
        Vector3 selectedScreenPos = CalculateScreenPosFromUnitHeadPosition(); 
        RepositionLaneChangeUI(selectedScreenPos);
        CheckLaneChangeFeasibility();
    }

    private void RepositionLaneChangeUI(Vector3 screenPos)
    {
        laneChangeUI.anchoredPosition = Helper.ScreenPointToWorldPosition(screenPos, canvas.scaleFactor);
        laneChangeUI.gameObject.SetActive(true);
        TurnOffLaneChangeButton(laneChangeUpButtonImg);
        TurnOffLaneChangeButton(laneChangeDownButtonImg);
        laneChangeUpButton.enabled = true;
        laneChangeDownButton.enabled = true;
    }

    public void AssignLaneChangeUp()
    {
        TurnOnLaneChangeButton(laneChangeUpButtonImg);
        if (laneChangeDownButton.enabled)
        {
            TurnOffLaneChangeButton(laneChangeDownButtonImg);
        }
    }

    public void AssignLaneChangeDown()
    {
        TurnOnLaneChangeButton(laneChangeDownButtonImg);
        if (laneChangeUpButton.enabled)
        {
            TurnOffLaneChangeButton(laneChangeUpButtonImg);
        }
    }

    private void CheckLaneChangeFeasibility()
    {
        if (currentLaneChangeSkillHolder.unit.isStunned())
        {
            BlockLaneChangeButton(laneChangeUpButton, laneChangeUpButtonImg);
            BlockLaneChangeButton(laneChangeDownButton, laneChangeDownButtonImg);
        }

        GridCoord[] gridsToCheck = currentLaneChangeSkillHolder.unit.GetAllCurrentGridPosition();

        // Check if lane changing upwards is possible
        if (gridsToCheck.Any(x => CheckIfLaneChangeIsBlocked(x, new GridCoord(0, 1))))
        {
            BlockLaneChangeButton(laneChangeUpButton, laneChangeUpButtonImg);
        }

        // Check if lane changing downwards is possible
        if (gridsToCheck.Any(x => CheckIfLaneChangeIsBlocked(x, new GridCoord(0, -1))))
        {
            BlockLaneChangeButton(laneChangeDownButton, laneChangeDownButtonImg);
        }

        return;
    }

    private bool CheckIfLaneChangeIsBlocked(GridCoord currentGrid, GridCoord moveGrid)
    {
        GridCoord nextGrid = Helper.AddGridCoords(currentGrid, moveGrid);

        bool isDivider = FieldGrid.GetDividerLaneNum() == nextGrid.y;
        bool isSidewalk = FieldGrid.GetBottomSidewalkLaneNum() == nextGrid.y || FieldGrid.GetTopSidewalkLaneNum() == nextGrid.y;
        bool vehicleInGrid = Helper.IsUnitOfTypeInTheWay(nextGrid, "Vehicle");
        bool bruteInGrid = Helper.IsUnitOfTypeInTheWay(nextGrid, "Brute");
        bool blockedBecauseOfMotorbike = false;
        if (currentLaneChangeSkillHolder.unit.GetName() != "Motorbike")
        {
            blockedBecauseOfMotorbike = CheckIfMotorbikeIsInbetweenLane(currentGrid, moveGrid);
        }
        else
        {
            blockedBecauseOfMotorbike = CheckIfMotorbikeIsBlockedByVehicleInItsLane(currentGrid, moveGrid);
        }

        return isDivider || isSidewalk || vehicleInGrid || bruteInGrid || blockedBecauseOfMotorbike;
    }

    private bool CheckIfMotorbikeIsInbetweenLane(GridCoord currentGrid, GridCoord moveGrid)
    {
        GridCoord nextGrid = Helper.AddGridCoords(currentGrid, moveGrid);
        int currentY = currentGrid.y;
        bool motorbikeInTheWay = false;

        // If moving upwards
        if (moveGrid.y == 1)
        {
            // Check its current grid for motorbike in its position, if there is, it should be blocking
            motorbikeInTheWay = FieldGrid.GetSingleGrid(currentGrid).GetListOfUnitsName().Contains("Motorbike");

            // If it is moving into the topmost lane (within each set of lanes), check if motorbike is in the next grid
            if (currentY == FieldGrid.GetDividerLaneNum() - 2 || currentY == FieldGrid.GetTopSidewalkLaneNum() - 2)
            {
                bool motorbikeInNextGrid = FieldGrid.GetSingleGrid(nextGrid).GetListOfUnitsName().Contains("Motorbike");
                motorbikeInTheWay = motorbikeInTheWay && motorbikeInNextGrid;
            }
        }
        else  // If moving downwards
        {
            // Check its next grid downwards for motorbike in its position, if there is, it should be blocking
            motorbikeInTheWay = FieldGrid.GetSingleGrid(nextGrid).GetListOfUnitsName().Contains("Motorbike");

            // If it is moving out from the topmost lane (within each set of lanes), check if motorbike is in the current grid
            if (currentY == FieldGrid.GetDividerLaneNum() - 1 || currentY == FieldGrid.GetTopSidewalkLaneNum() - 1)
            {
                bool motorbikeInNextGrid = FieldGrid.GetSingleGrid(currentGrid).GetListOfUnitsName().Contains("Motorbike");
                motorbikeInTheWay = motorbikeInTheWay && motorbikeInNextGrid;
            }
        }

        return motorbikeInTheWay;
    }

    private bool CheckIfMotorbikeIsBlockedByVehicleInItsLane(GridCoord currentGrid, GridCoord moveGrid)
    {
        // When moving up, it can only be blocked by a motorbike that is inbetween lane
        if (moveGrid.y == 1)
        {
            // TO BE IMPLEMENTED
            return false;
        }
        else  // Motorbike will be blocked from moving down if another vehicle is in its same grid 
        {
            var vehiclesInCurrentGrid = FieldGrid.GetSingleGrid(currentGrid).GetListOfUnitsWithGameObjectTag("Vehicle");
            var vehicleTags = vehiclesInCurrentGrid.Select(x => x.GetName());
            int numberOfMotorbikes = vehicleTags.Count(x => x == "Motorbike");
            int numberOfNonMotorbikes = vehicleTags.Count() - numberOfMotorbikes;

            // If there is another vehicle in the grid, then blocked
            if (numberOfNonMotorbikes > 0)
            {
                return true;
            }
            else if (numberOfMotorbikes >= 2) // If there is another motorbike in the grid, and no other vehicle, then blocked
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    private void TurnOnLaneChangeButton(RectTransform buttonImg)
    {
        buttonImg.GetComponent<Image>().color = laneChangeOnColor;
    }

    private void TurnOffLaneChangeButton(RectTransform buttonImg)
    {
        buttonImg.GetComponent<Image>().color = laneChangeOffColor;
    }

    private void BlockLaneChangeButton(Button button, RectTransform buttonImg)
    {
        button.enabled = false;
        buttonImg.GetComponent<Image>().color = laneChangeBlockColor;
    }

    private Vector3 CalculateScreenPosFromUnitHeadPosition()
    {
        GridCoord selectedGrid = currentLaneChangeSkillHolder.unit.GetCurrentHeadGridPosition();
        Vector3 selectedGridPos = FieldGrid.GetSingleGrid(selectedGrid).GetGridCentrePoint();
        return GameCamera.WorldToScreenPoint(selectedGridPos);
    }

    public Skill GetCurrentSkillHolder()
    {
        return currentLaneChangeSkillHolder;
    }

    public void ResetLaneChangeUI()
    {
        laneChangeUI.gameObject.SetActive(false);
    }

    public void RemoveSkillTarget()
    {
        laneChangeUI.gameObject.SetActive(false);
        TurnOffLaneChangeButton(laneChangeUpButtonImg);
        TurnOffLaneChangeButton(laneChangeDownButtonImg);
    }
}