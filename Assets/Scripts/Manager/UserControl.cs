using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UserControl : MonoBehaviour
{
    public Camera GameCamera;
    public GameObject Marker;
    public GameObject MarkerPrefab;

    private Unit m_Selected = null;

    private GraphicRaycasterManager graphicRaycasterManager;

    [SerializeField] private Toggle[] skillToggles;
    private SkillType skill_Selected = SkillType.None;
    private Dictionary<SkillType, int> skillCosts;
    private Dictionary<SkillType, ISkill> skillTargets = new Dictionary<SkillType, ISkill>();

    private LaneChangeManager laneChangeManager;
    private bool laneChangeHighlighted = false;

    private UIMain uiMain;
    private int currentSkillOrbCount = 0;
    private int consumedSkillOrbCount = 0;
    private int maxOrbCount = 10;

    [SerializeField] private Button[] carChoosingButtons;
    [SerializeField] private Image invalidVehicleDropSelectionImg;
    [SerializeField] private Image validVehicleDropSelectionImg;
    [SerializeField] private RectTransform invalidVehicleLaneSelectionImg;
    [SerializeField] private RectTransform validVehicleLaneSelectionImg;
    private VehicleSpawner vehicleSpawner;
    private ISkill tempSelectVehicleSkill;
    private List<Unit> calledInVehicles;
    GridCoord currentVehicleSkillHoverGrid;

    private void Awake()
    {
        uiMain = UIMain.Instance;
        laneChangeManager = gameObject.GetComponent<LaneChangeManager>();
        vehicleSpawner = gameObject.GetComponent<VehicleSpawner>();
        graphicRaycasterManager = GameObject.Find("Canvas").GetComponent<GraphicRaycasterManager>();

        Marker.SetActive(false);
        for (int i = 0; i < skillToggles.Length; i++)
        {
            int tmp = i;
            skillToggles[i].onValueChanged.AddListener(delegate { HandleSelectSkill(tmp); });
        }

        skillCosts = new Dictionary<SkillType, int> {
            { SkillType.Assassinate, 8 },
            { SkillType.CallInVeh, 3 },
            { SkillType.AirDropVeh, 10 },
            { SkillType.LaneChange, 2 },
            { SkillType.BoostUnit, 3 },
            { SkillType.DisableUnit, 3 }
        };

        for (int i = 0; i < carChoosingButtons.Length; i++)
        {
            int tmp = i;
            carChoosingButtons[i].onClick.AddListener(delegate { HandleSelectCar(tmp); });
        }
        tempSelectVehicleSkill = null;
        currentVehicleSkillHoverGrid = new GridCoord(0, 0);
        calledInVehicles = new List<Unit>();
    }

    private bool HaveVehiclesBlockingAirDrop(GridCoord targetGrid, Unit unit)
    {
        int vehicleSize = unit.GetSize();
        int direction = targetGrid.y < FieldGrid.GetDividerLaneNum() ? 1 : -1;
        bool blocked = false;

        for (int i = 0; i < vehicleSize; i++)
        {
            GridCoord checkGrid = Helper.AddGridCoords(targetGrid, new GridCoord(i * direction, 0));
            List<string> gameObjectsTagInGrid = FieldGrid.GetSingleGrid(checkGrid).GetListOfUnitsGameObjectTag();
            if (gameObjectsTagInGrid.Contains("Vehicle"))
            {
                blocked = true;
                break;
            }
        }
        return blocked;
    }

    private void Update()
    {
        if (tempSelectVehicleSkill != null)
        {
            GridCoord currentTargetedGrid = FieldGrid.GetGridCoordFromWorldPosition(GameCamera.ScreenToWorldPoint(Input.mousePosition));
            if (IsWithinPlayableX(currentTargetedGrid.x))
            {
                if (skill_Selected == SkillType.AirDropVeh)
                {
                    if (!Helper.IsEqualGridCoords(currentVehicleSkillHoverGrid, currentTargetedGrid))
                    {
                        Vector3 mouseOverGridPos = FieldGrid.GetSingleGrid(currentTargetedGrid).GetGridCentrePoint();
                        if (!IsTargetedGridInALane(currentTargetedGrid.y) || HaveVehiclesBlockingAirDrop(currentTargetedGrid, tempSelectVehicleSkill.unit))
                        {
                            invalidVehicleDropSelectionImg.transform.position = GameCamera.WorldToScreenPoint(mouseOverGridPos);
                            invalidVehicleDropSelectionImg.gameObject.SetActive(true);
                            validVehicleDropSelectionImg.gameObject.SetActive(false);
                        }
                        else
                        {
                            validVehicleDropSelectionImg.transform.position = GameCamera.WorldToScreenPoint(mouseOverGridPos);
                            validVehicleDropSelectionImg.gameObject.SetActive(true);
                            invalidVehicleDropSelectionImg.gameObject.SetActive(false);
                        }
                        currentVehicleSkillHoverGrid = currentTargetedGrid;
                    }
                }
                else if (skill_Selected == SkillType.CallInVeh)
                {
                    if (currentVehicleSkillHoverGrid.y != currentTargetedGrid.y)
                    {
                        GridCoord laneCentreGrid = new GridCoord(FieldGrid.GetMaxLength() / 2, currentTargetedGrid.y);
                        Vector3 laneCentreGridPos = FieldGrid.GetSingleGrid(laneCentreGrid).GetGridCentrePoint();
                        if (IsTargetedGridInALane(currentTargetedGrid.y) && CheckNoExistingCalledInVehInSameLane(currentTargetedGrid.y))
                        {
                            validVehicleLaneSelectionImg.transform.position = GameCamera.WorldToScreenPoint(laneCentreGridPos);
                            validVehicleLaneSelectionImg.gameObject.SetActive(true);
                            invalidVehicleLaneSelectionImg.gameObject.SetActive(false);
                        }
                        else
                        {
                            invalidVehicleLaneSelectionImg.transform.position = GameCamera.WorldToScreenPoint(laneCentreGridPos);
                            invalidVehicleLaneSelectionImg.gameObject.SetActive(true);
                            validVehicleLaneSelectionImg.gameObject.SetActive(false);
                        }
                        currentVehicleSkillHoverGrid = currentTargetedGrid;
                    }
                }
                else
                {
                    Debug.Log("State should not be in locator mode. Skill selected isn't call-in veh or air drop vehicle.");
                }
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            HandleSelection();
        }

        if (Input.GetMouseButtonDown(1))
        {
            HandleDeselection();
        }

        MarkerHandling();
    }

    private bool CheckNoExistingCalledInVehInSameLane(int gridY)
    {
        return calledInVehicles.All(x => x.GetCurrentHeadGridPosition().y != gridY);
    }

    // Handle displaying the marker above the unit that is currently selected (or hiding it if no unit is selected)
    void MarkerHandling()
    {
        if (Marker == null)
        {
            Marker = Instantiate(MarkerPrefab, Vector3.down, MarkerPrefab.transform.rotation);
        }

        if (m_Selected == null && Marker.activeInHierarchy)
        {
            Marker.SetActive(false);
            Marker.transform.SetParent(null);
        }
        else if (m_Selected != null && Marker.transform.parent != m_Selected.transform)
        {
            Marker.SetActive(true);
            Marker.transform.SetParent(m_Selected.transform, true);
            Marker.transform.localPosition = Vector3.zero;
        }
    }

    public void HandleDeselection()
    {
        m_Selected = null;
        UIMain.Instance.SetNewInfoContent(null);

        ResetCurrentSelectedButton();
        tempSelectVehicleSkill = null;
    }

    public void HandleSelection()
    {
        if (graphicRaycasterManager.IsSelectingUI())
        {
            Debug.Log("Selected UI Element... ignoring...");
            return;
        }

        if (IsSelectingVehicleSkill())
        {
            if (tempSelectVehicleSkill != null)
            {
                Debug.Log("[HandleSelection] In locator mode...");
                if (graphicRaycasterManager.HasSelectedValidLocatorUI())
                {
                    Debug.Log("[VehicleSkill] Has selected valid grid...");
                    GridCoord currentTargetedGrid = FieldGrid.GetGridCoordFromWorldPosition(GameCamera.ScreenToWorldPoint(Input.mousePosition));
                    if (skill_Selected == SkillType.CallInVeh)
                    {
                        if (currentTargetedGrid.y < FieldGrid.GetDividerLaneNum())
                        {
                            currentTargetedGrid.x = FieldGrid.GetMaxLength() - FieldGrid.GetFieldBuffer();
                        }
                        else
                        {
                            currentTargetedGrid.x = FieldGrid.GetFieldBuffer() - 1;
                        }
                    }
                    tempSelectVehicleSkill.UpdateGridCoordAction(currentTargetedGrid);
                    skillTargets.Add(skill_Selected, tempSelectVehicleSkill);
                    tempSelectVehicleSkill = null;

                    Debug.Log($"Readying Skill {skill_Selected} for execution, target at ({currentTargetedGrid.x}, {currentTargetedGrid.y})");
                    HighlightCurrentSelectedButton();
                    ResetCurrentSelectedButton();
                }
                else
                {
                    Debug.Log("[VehicleSkill] Has selected invalid grid...");
                }
            }
            else 
            {
                Debug.Log("[HandleSelection] Not in locator mode yet, cancelling player vehicle skill...");
                skillToggles[(int)skill_Selected].isOn = false;
            }
        }

        Vector3 screenPos = Input.mousePosition;
        var ray = GameCamera.ScreenPointToRay(screenPos);

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            //the collider could be children of the unit, so we make sure to check in the parent
            var unit = hit.collider.GetComponentInParent<Unit>();
            m_Selected = unit;

            //check if the hit object have a IUIInfoContent to display in the UI
            //if there is none, this will be null, so this will hid the panel if it was displayed
            var uiInfo = hit.collider.GetComponentInParent<UIMain.IUIInfoContent>();
            UIMain.Instance.SetNewInfoContent(uiInfo);

            if (m_Selected != null && skill_Selected != SkillType.None)
            {
                //Debug.Log($"{skill_Selected} has been targeted at a unit, saving skill target...");
                // Register unit and skill command to be dispatched after player turn ends. 
                // This is to allow for cancellation of commands before turn is officially ended.
                // [TEMP] Each skill can only be used once, so a dictionary is used
                ISkill currentSkill = GenerateSkill(skill_Selected, m_Selected);

                // Assassinate only works on enemy units
                if (skill_Selected == SkillType.Assassinate && m_Selected.gameObject.tag != "Enemy")
                {
                    return;
                }

                // Lane Change only works on vehicles
                if (skill_Selected == SkillType.LaneChange)
                {
                    if (m_Selected.gameObject.tag != "Vehicle")
                    {
                        return;
                    }
                    laneChangeManager.RegisterLaneChangeTarget(currentSkill);
                }
                else if (!IsSelectingVehicleSkill())
                {
                    // After the skill has been assigned a targeted unit, the button will become highlighted in green, 
                    // and it will not flash yellow.
                    //Debug.Log("Indicate successful targeting of skill, resetting active skill...");
                    skillTargets.Add(skill_Selected, currentSkill);
                    HighlightCurrentSelectedButton();
                    ResetCurrentSelectedButton();
                    skill_Selected = SkillType.None;
                }
            }
        }
    }

    private void HandleSelectSkill(int skillNum)
    {
        // If button is selected
        if (skillToggles[skillNum].isOn)
        {
            Debug.Log($"Skill {(SkillType)skillNum} has been selected!");
            // Remove previously selected skill from active state and set this as current active skill
            ResetPreviousSelectedButton((SkillType)skillNum);
            SetActiveSkill((SkillType)skillNum);

            // If current active skill was previously assigned a target already, reselection will cancel that target
            if (skillToggles[skillNum].GetComponentInChildren<Outline>().enabled == true)
            {
                uiMain.ReactivateSkillOrb(skillCosts[(SkillType)skillNum]);
                consumedSkillOrbCount -= skillCosts[(SkillType)skillNum];
                ResetHighlightCurrentSelectedButton();
                RemoveSkillTarget((SkillType)skillNum);
            }

            // Set state of deactivated skill orbs based on selected skill
            uiMain.DeactivateSkillOrb(skillCosts[(SkillType)skillNum]);
            consumedSkillOrbCount += skillCosts[(SkillType)skillNum];
        }
        else  // If button is deselected
        {
            Debug.Log($"Skill {(SkillType)skillNum} has been deselected!");
            if (skillToggles[skillNum].GetComponentInChildren<Outline>().enabled != true)
            {
                uiMain.ReactivateSkillOrb(skillCosts[(SkillType)skillNum]);
                consumedSkillOrbCount -= skillCosts[(SkillType)skillNum];
            }
            ClearActiveSkill();
        }
    }

    public void RefreshSkillsUI()
    {
        consumedSkillOrbCount = 0;
        uiMain.RefreshSkillOrbBar();
        ResetSkillBar();
    }

    private void SetActiveSkill(SkillType skill)
    {
        skill_Selected = skill;
        if (IsSelectingVehicleSkill())
        {
            uiMain.ActivateVehicleSelectionUI();
        }
    }

    private void ClearActiveSkill()
    {
        if (IsSelectingVehicleSkill())
        {
            uiMain.DeactivateVehicleSelectionUI();
            tempSelectVehicleSkill = null;
            if (skillToggles[(int)SkillType.CallInVeh].GetComponentInChildren<Outline>().enabled != true)
            {
                invalidVehicleLaneSelectionImg.gameObject.SetActive(false);
                validVehicleLaneSelectionImg.gameObject.SetActive(false);
            }
            if (skillToggles[(int)SkillType.AirDropVeh].GetComponentInChildren<Outline>().enabled != true)
            {
                validVehicleDropSelectionImg.gameObject.SetActive(false);
                invalidVehicleDropSelectionImg.gameObject.SetActive(false);
            }
        }
        skill_Selected = SkillType.None;
    }

    private void ResetCurrentSelectedButton()
    {
        if (skill_Selected != SkillType.None)
        {
            skillToggles[(int)skill_Selected].isOn = false;
        }
    }

    public void UICancelCurrentSkillSelection()
    {
        ResetCurrentSelectedButton();
    }

    private void ResetPreviousSelectedButton(SkillType skill)
    {
        if (skill_Selected != SkillType.None && skill_Selected != skill)
        {
            skillToggles[(int)skill_Selected].isOn = false;
        }
    }

    private void HighlightCurrentSelectedButton()
    {
        if (skill_Selected != SkillType.None)
        {
            skillToggles[(int)skill_Selected].GetComponentInChildren<Outline>().enabled = true;
        }
        UpdateSkillTogglesFunctionality();
    }

    private void ResetHighlightCurrentSelectedButton()
    {
        if (skill_Selected != SkillType.None)
        {
            skillToggles[(int)skill_Selected].GetComponentInChildren<Outline>().enabled = false;
            UpdateSkillTogglesFunctionality();
        }
    }

    public void DispatchSkillCommands()
    {
        int skillOrbsConsumed = 0;
        foreach (KeyValuePair<SkillType, ISkill> entry in skillTargets)
        {
            entry.Value.Execute();
            skillOrbsConsumed += skillCosts[entry.Key];
            currentSkillOrbCount -= skillCosts[entry.Key];

            if (entry.Key == SkillType.CallInVeh)
            {
                calledInVehicles.Add(entry.Value.unit);
            }

        }
        uiMain.RemoveSkillOrb(skillOrbsConsumed);
        ResetSkillTargets();
    }

    private void RemoveSkillTarget(SkillType skill)
    {
        if (skillTargets.ContainsKey(skill))
        {
            skillTargets.Remove(skill);
        }
        if (skill == SkillType.LaneChange)
        {
            laneChangeHighlighted = false;
            laneChangeManager.RemoveSkillTarget();
        } 
        else if (skill == SkillType.CallInVeh)
        {
            validVehicleLaneSelectionImg.gameObject.SetActive(false);
        } 
        else if (skill == SkillType.AirDropVeh)
        {
            validVehicleDropSelectionImg.gameObject.SetActive(false);
        }
    }

    private void ResetSkillTargets()
    {
        if (skillTargets != null)
        {
            skillTargets.Clear();
        }
    }

    private ISkill GenerateSkill(SkillType skillType, Unit unit)
    {
        switch (skillType)
        {
            case SkillType.Assassinate:
                return new Assassinate(unit);
            case SkillType.LaneChange:
                return new LaneChange(unit);
            case SkillType.BoostUnit:
                return new BoostUnit(unit);
            case SkillType.DisableUnit:
                return new DisableUnit(unit);
            case SkillType.CallInVeh:
                return new CallInVehicle(unit);
            case SkillType.AirDropVeh:
                return new AirDropVehicle(unit);
        }
        return null;
    }

    public void LaneChangeUp()
    {
        CheckAndCompleteLaneChangeSelection();
        skillTargets[SkillType.LaneChange].UpdateGridCoordAction(new GridCoord(0, 1));
        laneChangeManager.AssignLaneChangeUp();
    }

    public void LaneChangeDown()
    {
        CheckAndCompleteLaneChangeSelection();
        skillTargets[SkillType.LaneChange].UpdateGridCoordAction(new GridCoord(0, -1));
        laneChangeManager.AssignLaneChangeDown();
    }

    private void CheckAndCompleteLaneChangeSelection()
    {
        if (laneChangeHighlighted == false && laneChangeManager.GetCurrentSkillHolder() != null)
        {
            HighlightCurrentSelectedButton();
            ResetCurrentSelectedButton();

            skillTargets.Add(SkillType.LaneChange, laneChangeManager.GetCurrentSkillHolder());
            laneChangeHighlighted = true;
        }
    }

    public void AddSkillOrb(int count)
    {
        currentSkillOrbCount = Mathf.Min(maxOrbCount, currentSkillOrbCount + count);
        uiMain.AddSkillOrb(count);
    }

    public void ResetSkillBar()
    {
        for (int i = 0; i < skillToggles.Length; i++)
        {
            skillToggles[i].isOn = false;
            skillToggles[i].GetComponentInChildren<Outline>().enabled = false;
        }
        laneChangeManager.ResetLaneChangeUI();
        ResetPlayerVehicleSelectorUI();
    }

    private void ResetPlayerVehicleSelectorUI()
    {
        uiMain.DeactivateVehicleSelectionUI();
        validVehicleDropSelectionImg.gameObject.SetActive(false);
        validVehicleLaneSelectionImg.gameObject.SetActive(false);
    }

    private void HandleSelectCar(int carNum)
    {
        if (IsSelectingVehicleSkill())
        {
            Unit selectedVehicle = vehicleSpawner.GetPlayerSkillVehicle(carNum).GetComponent<Unit>();
            tempSelectVehicleSkill = GenerateSkill(skill_Selected, selectedVehicle);
            uiMain.DeactivateVehicleSelectionUI();
        }
    }

    public void UpdateSkillTogglesFunctionality()
    {
        Debug.Log($"[UserControl] Current skill orbs: {currentSkillOrbCount}. Consumed: {consumedSkillOrbCount}");
        int skillOrbCount = currentSkillOrbCount - consumedSkillOrbCount;
        for (int i = 0; i < skillToggles.Length; i++)
        {
            if (skillCosts[(SkillType)i] > skillOrbCount)
            {
                if (skillToggles[i].isOn == false && skillToggles[i].GetComponentInChildren<Outline>().enabled != true)
                {
                    DisableSkillButton(skillToggles[i]);
                }
            }
            else
            {
                EnableSkillButton(skillToggles[i]);
            }
        }
    }

    private void DisableSkillButton(Toggle skillButton)
    {
        if (skillButton.enabled == true)
        {
            skillButton.enabled = false;
            ColorBlock cb = skillButton.colors;
            cb.normalColor = Color.gray;
            skillButton.colors = cb;
        }
    }

    private void EnableSkillButton(Toggle skillButton)
    {
        // Only apply if button is not enabled
        if (skillButton.enabled != true)
        {
            skillButton.enabled = true;
            ColorBlock cb = skillButton.colors;
            cb.normalColor = Color.white;
            skillButton.colors = cb;
        }
    }

    private bool IsSelectingVehicleSkill()
    {
        return skill_Selected == SkillType.CallInVeh || skill_Selected == SkillType.AirDropVeh;
    }

    private bool IsTargetedGridInALane(int gridY)
    {
        return gridY != FieldGrid.GetDividerLaneNum() && gridY < FieldGrid.GetTopSidewalkLaneNum() && gridY > FieldGrid.GetBottomSidewalkLaneNum();
    }

    private bool IsWithinPlayableX(int gridX)
    {
        return gridX >= FieldGrid.GetFieldBuffer() && gridX < (FieldGrid.GetMaxLength() - FieldGrid.GetFieldBuffer());
    }

    public void UpdateCalledInVehicles()
    {
        calledInVehicles = calledInVehicles.Where(x => !IsWithinPlayableX(x.GetCurrentHeadGridPosition().x)).ToList();
    }

}