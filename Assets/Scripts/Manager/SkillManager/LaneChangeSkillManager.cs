using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LaneChangeSkillManager : ISkillManager
{
    public SkillType m_SkillType { get; set; }
    public ISkill m_Skill { get; set; }
    public int m_SkillCost { get; set; }
    public bool m_LockedIn { get; set; }

    private SkillSoundManager skillSoundManager;
    private Camera GameCamera;
    private Canvas canvas;
    private GraphicRaycasterManager graphicRaycasterManager;
    private RectTransform laneChangeUI;
    private Button laneChangeUpButton;
    private Button laneChangeDownButton;
    private Image laneChangeUpButtonImg;
    private Image laneChangeDownButtonImg;

    private Color laneChangeOnColor = new Color(0.07f, 1, 0, .5f);
    private Color laneChangeOffColor = new Color(0.35f, 0.35f, 0.35f, .5f);
    private Color laneChangeBlockColor = new Color(1, 0, 0, .5f);

    public LaneChangeSkillManager()
    {
        m_SkillType = SkillType.LaneChange;
        m_Skill = null;
        m_SkillCost = 2;
        m_LockedIn = false;

        skillSoundManager = GameObject.Find("SkillSoundManager").GetComponent<SkillSoundManager>();
        GameCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        graphicRaycasterManager = GameObject.Find("Canvas").GetComponent<GraphicRaycasterManager>();

        laneChangeUI = GameObject.Find("LaneChangeDirections").GetComponent<RectTransform>();
        laneChangeUpButton = GameObject.Find("LaneChangeUp").GetComponent<Button>();
        laneChangeDownButton = GameObject.Find("LaneChangeDown").GetComponent<Button>();
        laneChangeUpButtonImg = GameObject.Find("LaneChangeUpColor").GetComponent<Image>();
        laneChangeDownButtonImg = GameObject.Find("LaneChangeDownColor").GetComponent<Image>();

        laneChangeUpButton.onClick.AddListener(delegate { AssignLaneChangeUp(); });
        laneChangeDownButton.onClick.AddListener(delegate { AssignLaneChangeDown(); });
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
            if (selectedLaneChangeUI != null)
            {
                if (selectedLaneChangeUI.GetComponent<Button>().enabled)
                {
                    m_LockedIn = true;
                    completedSkillSelection = true;
                }
                else
                {
                    skillSoundManager.PlayInvalidSelection();
                }
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
    }

    public void ProcessSelection(Unit selectedUnit)
    {
        // Lane Change only works on vehicles
        if (selectedUnit != null && selectedUnit.gameObject.tag == "Vehicle")
        {
            UpdateSkillUnit(selectedUnit);
            Vector3 selectedScreenPos = GameCamera.WorldToScreenPoint(selectedUnit.gameObject.transform.position);
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
            skillSoundManager.PlaySkillConfirm();
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
            skillSoundManager.PlaySkillConfirm();
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

        if (isDivider) Debug.Log("Lane Change Blocked by divider!");
        if (isSidewalk) Debug.Log("Lane Change Blocked by sidewalk!");
        if (vehicleInGrid) Debug.Log("Lane Change Blocked by vehicle in the way!");
        if (enemyInGrid) Debug.Log("Lane Change Blocked by enemy in the way!");

        return isDivider || isSidewalk || vehicleInGrid || enemyInGrid;
    }

    private void TurnOnLaneChangeButton(Image buttonImg)
    {
        buttonImg.color = laneChangeOnColor;
    }

    private void TurnOffLaneChangeButton(Image buttonImg)
    {
        buttonImg.color = laneChangeOffColor;
    }

    private void BlockLaneChangeButton(Button button, Image buttonImg)
    {
        button.enabled = false;
        buttonImg.color = laneChangeBlockColor;
    }

    public void ExecuteSkill()
    {
        if (m_LockedIn)
        {
            skillSoundManager.PlayLaneChange();
            m_Skill.Execute();
            RemoveSkillTarget();
        }
    }

    public void ActivateSkillUI()
    {
        return;
    }

    public string GetExecuteLog()
    {
        Unit targetUnit = m_Skill.unit.GetComponent<Unit>();
        return $"Lane Change Skill used on {targetUnit.GetName()} at Grid [{targetUnit.GetCurrentHeadGridPosition().x}, {targetUnit.GetCurrentHeadGridPosition().y}].";
    }

    public void RepositionSkillMarkerUI()
    {
        return;
    }
}