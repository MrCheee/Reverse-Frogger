using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LaneChangeSkillManager : ISkillManager
{
    public SkillType m_SkillType { get; set; }
    public ISkill m_Skill { get; set; }
    public int m_SkillCost { get; set; }
    public bool m_LockedIn { get; set; }

    private Camera GameCamera;
    private Canvas canvas;
    private GraphicRaycasterManager graphicRaycasterManager;
    private RectTransform laneChangeUI;
    private Button laneChangeUpButton;
    private Button laneChangeDownButton;
    private RectTransform laneChangeUpButtonImg;
    private RectTransform laneChangeDownButtonImg;

    private Color laneChangeOnColor;
    private Color laneChangeOffColor;
    private Color laneChangeBlockColor;

    GameObject SkillMarker;

    public LaneChangeSkillManager()
    {
        m_SkillType = SkillType.LaneChange;
        m_Skill = null;
        m_SkillCost = 2;
        m_LockedIn = false;

        GameCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        graphicRaycasterManager = GameObject.Find("Canvas").GetComponent<GraphicRaycasterManager>();

        laneChangeUI = GameObject.Find("LaneChangeDirections").GetComponent<RectTransform>();
        laneChangeUpButton = GameObject.Find("LaneChangeUp").GetComponent<Button>();
        laneChangeDownButton = GameObject.Find("LaneChangeDown").GetComponent<Button>();
        laneChangeUpButtonImg = GameObject.Find("LaneChangeUpImage").GetComponent<RectTransform>();
        laneChangeDownButtonImg = GameObject.Find("LaneChangeDownImage").GetComponent<RectTransform>();

        laneChangeUpButton.onClick.AddListener(delegate { AssignLaneChangeUp(); });
        laneChangeDownButton.onClick.AddListener(delegate { AssignLaneChangeDown(); });

        laneChangeOnColor = new Color(255, 182, 0, 0.5f);
        laneChangeOffColor = new Color(161, 161, 161, 0.5f);
        laneChangeBlockColor = new Color(255, 0, 0, 0.5f);

        SkillMarker = GameObject.Find("LaneChangeSkillMarker");
    }

    public void InitialiseSkill(Unit unit)
    {
        m_Skill = new LaneChange(unit);
    }

    public void UpdateSkillUnit(Unit unit)
    {
        if (m_Skill == null)
        {
            InitialiseSkill(unit);
        }
        else
        {
            m_Skill.unit = unit;
        }
    }

    public void ExecuteUpdateCheck()
    {
        return;
    }

    public bool ProcessSelectionAndCompletedSkill(Unit selectedUnit)
    {
        bool completedSkillSelection = false;
        if (m_Skill != null)
        {
            GameObject selectedLaneChangeUI = graphicRaycasterManager.GetSelectedLaneChangeUI();
            if (selectedLaneChangeUI != null && selectedLaneChangeUI.GetComponent<Button>().enabled)
            {
                m_LockedIn = true;
                completedSkillSelection = true;
                SkillMarker.transform.position = m_Skill.unit.transform.position;
                SkillMarker.SetActive(true);
            }
        }
        return completedSkillSelection;
    }

    public bool CancelSelection(Unit selectedUnit)
    {
        return false;
    }

    public void DeactivateSkillUI()
    {
        laneChangeUI.gameObject.SetActive(false);
    }

    public void RemoveSkillTarget()
    {
        m_Skill = null;
        m_LockedIn = false;
        laneChangeUI.gameObject.SetActive(false);
        TurnOffLaneChangeButton(laneChangeUpButtonImg);
        TurnOffLaneChangeButton(laneChangeDownButtonImg);
        SkillMarker.SetActive(false);
    }

    public void ProcessSelection(Unit selectedUnit)
    {
        // Lane Change only works on vehicles
        if (selectedUnit != null && selectedUnit.gameObject.tag == "Vehicle")
        {
            UpdateSkillUnit(selectedUnit);
            Vector3 selectedScreenPos = GameCamera.WorldToScreenPoint(selectedUnit.gameObject.transform.position);
            //Vector3 selectedScreenPos = CalculateScreenPosFromUnitHeadPosition();
            RepositionLaneChangeUI(selectedScreenPos);
            CheckLaneChangeFeasibility();
        }
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
        if (m_Skill != null)
        {
            m_Skill.UpdateGridCoordAction(new GridCoord(0, 1));
        }
        TurnOnLaneChangeButton(laneChangeUpButtonImg);
        if (laneChangeDownButton.enabled)
        {
            TurnOffLaneChangeButton(laneChangeDownButtonImg);
        }
    }

    public void AssignLaneChangeDown()
    {
        if (m_Skill != null)
        {
            m_Skill.UpdateGridCoordAction(new GridCoord(0, -1));
        }
        TurnOnLaneChangeButton(laneChangeDownButtonImg);
        if (laneChangeUpButton.enabled)
        {
            TurnOffLaneChangeButton(laneChangeUpButtonImg);
        }
    }

    private void CheckLaneChangeFeasibility()
    {
        if (m_Skill.unit.isStunned())
        {
            BlockLaneChangeButton(laneChangeUpButton, laneChangeUpButtonImg);
            BlockLaneChangeButton(laneChangeDownButton, laneChangeDownButtonImg);
        }

        GridCoord[] gridsToCheck = m_Skill.unit.GetAllCurrentGridPosition();

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
        bool vehicleInGrid = Helper.IsVehicleInTheWay(nextGrid);
        bool enemyInGrid = Helper.IsEnemyInTheWay(nextGrid);
        bool blockedBecauseOfMotorbike = false;
        if (m_Skill.unit.GetName() != "Motorbike")
        {
            blockedBecauseOfMotorbike = CheckIfMotorbikeIsInbetweenLane(currentGrid, moveGrid);
        }
        else
        {
            blockedBecauseOfMotorbike = CheckIfMotorbikeIsBlockedByVehicleInItsLane(currentGrid, moveGrid);
        }

        if (isDivider) Debug.Log("Lane Change Blocked by divider!");
        if (isSidewalk) Debug.Log("Lane Change Blocked by sidewalk!");
        if (vehicleInGrid) Debug.Log("Lane Change Blocked by vehicle in the way!");
        if (enemyInGrid) Debug.Log("Lane Change Blocked by enemy in the way!");
        if (blockedBecauseOfMotorbike) Debug.Log("Lane Change Blocked by motorbike in the way!");

        return isDivider || isSidewalk || vehicleInGrid || enemyInGrid || blockedBecauseOfMotorbike;
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
        GridCoord selectedGrid = m_Skill.unit.GetCurrentHeadGridPosition();
        Vector3 selectedGridPos = FieldGrid.GetSingleGrid(selectedGrid).GetGridCentrePoint();
        return GameCamera.WorldToScreenPoint(selectedGridPos);
    }

    public void ExecuteSkill()
    {
        if (m_LockedIn)
        {
            m_Skill.Execute();
            RemoveSkillTarget();
            SkillMarker.SetActive(false);
        }
    }

    public void ActivateSkillUI()
    {
        return;
    }
}