using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserControl : MonoBehaviour
{
    public Camera GameCamera;
    public GameObject Marker;
    public GameObject MarkerPrefab;

    private Unit m_Selected = null;

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

    //[SerializeField] private Button[] carChoosingButtons;
    //[SerializeField] private RectTransform carChoosingUI;
    //private VehicleSpawner vehicleSpawner;

    private void Awake()
    {
        uiMain = UIMain.Instance;
        laneChangeManager = gameObject.GetComponent<LaneChangeManager>();
        //vehicleSpawner = gameObject.GetComponent<VehicleSpawner>();

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

        //for (int i = 0; i < carChoosingButtons.Length; i++)
        //{
        //    int tmp = i;
        //    carChoosingButtons[i].onClick.AddListener(delegate { HandleSelectCar(tmp); });
        //}
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleSelection();
        }

        MarkerHandling();
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

    public void HandleSelection()
    {
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
                Debug.Log($"{skill_Selected} has been targeted at a unit, saving skill target...");
                // Register unit and skill command to be dispatched after player turn ends. 
                // This is to allow for cancellation of commands before turn is officially ended.
                // [TEMP] Each skill can only be used once, so a dictionary is used
                Skill currentSkill = GenerateSkill(skill_Selected, m_Selected);

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
                else
                {
                    skillTargets.Add(skill_Selected, currentSkill);
                    // After the skill has been assigned a targeted unit, the button will become highlighted in green, 
                    // and it will not flash yellow.
                    Debug.Log("Indicate successful targeting of skill, resetting active skill...");
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
            Debug.Log($"Skill {skillNum+1} has been selected!");
            uiMain.DeactivateSkillOrb(skillCosts[(SkillType)skillNum]);
            consumedSkillOrbCount += skillCosts[(SkillType)skillNum];

            // Remove previously selected skill from active state and set this as current active skill
            ResetCurrentSelectedButton();
            SetActiveSkill((SkillType)skillNum);

            // If current active skill was previously assigned a target already, reselection will cancel that target
            if (skillToggles[skillNum].GetComponentInChildren<Outline>().enabled == true)
            {
                uiMain.ReactivateSkillOrb(skillCosts[(SkillType)skillNum]);
                consumedSkillOrbCount -= skillCosts[(SkillType)skillNum];
            }
            ResetHighlightCurrentSelectedButton();
            RemoveSkillTarget((SkillType)skillNum);
        }
        else  // If button is deselected
        {
            Debug.Log($"Skill {skillNum+1} has been deselected!");
            if (skillToggles[skillNum].GetComponentInChildren<Outline>().enabled != true)
            {
                uiMain.ReactivateSkillOrb(skillCosts[(SkillType)skillNum]);
                consumedSkillOrbCount -= skillCosts[(SkillType)skillNum];
            }
            ClearActiveSkill();
        }
        UpdateSkillTogglesFunctionality();
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
    }

    private void ClearActiveSkill()
    {
        skill_Selected = SkillType.None;
    }

    private void ResetCurrentSelectedButton()
    {
        if (skill_Selected != SkillType.None)
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
    }

    private void ResetHighlightCurrentSelectedButton()
    {
        if (skill_Selected != SkillType.None)
        {
            skillToggles[(int)skill_Selected].GetComponentInChildren<Outline>().enabled = false;
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
    }

    private void ResetSkillTargets()
    {
        if (skillTargets != null)
        {
            skillTargets.Clear();
        }
    }

    private Skill GenerateSkill(SkillType skillType, Unit unit)
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
    }

    private void HandleSelectCar(int carNum)
    {
        int adjustedCarNum = carNum - 1;
        if (adjustedCarNum < 0)
        {
            adjustedCarNum = UnityEngine.Random.Range(0, Enum.GetNames(typeof(VehicleType)).Length);
        }
        
    }

    public void UpdateSkillTogglesFunctionality()
    {
        Debug.Log($"Current skill orbs: {currentSkillOrbCount}. Consumed: {consumedSkillOrbCount}");
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
}