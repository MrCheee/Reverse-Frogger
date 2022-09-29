using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserControl : MonoBehaviour
{
    public Camera GameCamera;
    public GameObject Marker;
    public GameObject MarkerPrefab;

    [SerializeField] private Canvas canvas;
    [SerializeField] private RectTransform laneChangeUI;

    private Unit m_Selected = null;

    [SerializeField] private Toggle[] skillToggles;
    private SkillType skill_Selected = SkillType.None;
    private Dictionary<SkillType, ISkill> skillTargets = new Dictionary<SkillType, ISkill>();

    private void Start()
    {
        Marker.SetActive(false);
        
        for (int i = 0; i < skillToggles.Length; i++)
        {
            int tmp = i;
            skillToggles[i].onValueChanged.AddListener(delegate { HandleSelectSkill(tmp); });
        }
    }

    private void Update()
    {
        //Vector2 move = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        //GameCamera.transform.position = GameCamera.transform.position + new Vector3(move.y, 0, -move.x) * PanSpeed * Time.deltaTime;

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
                    RepositionLaneChangeUI(screenPos);
                    CheckLaneChangeFeasibility();
                    Debug.Log("Generated Lane Change UI at targeted unit's spot...");
                }

                skillTargets.Add(skill_Selected, currentSkill);

                // After a skill has been assigned a targeted unit, the button will become highlighted in green, 
                // and it will not flash yellow.
                Debug.Log("Indicate successful targeting of skill, resetting active skill...");
                HighlightCurrentSelectedButton();
                ResetCurrentSelectedButton();
                skill_Selected = SkillType.None;
            }
        }
    }

    private void HandleSelectSkill(int skillNum)
    {
        Debug.Log(skillNum);
        // If button is selected
        if (skillToggles[skillNum].isOn)
        {
            Debug.Log($"Skill {skillNum} has been clicked!");
            // Remove previously selected skill from active state and set this as current active skill
            ResetCurrentSelectedButton();
            SetActiveSkill((SkillType)skillNum);

            // If current active skill was previously assigned a target already, reselection will cancel that target
            ResetHighlightCurrentSelectedButton();
            RemoveSkillTarget((SkillType)skillNum);
        }
        else  // If button is deselected
        {
            Debug.Log($"Skill {skillNum} has been unclicked!");
            ClearActiveSkill();
        }
    }

    private void SetActiveSkill(SkillType skill)
    {
        skill_Selected = skill;
        Debug.Log(skill_Selected);
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
        foreach (KeyValuePair<SkillType, ISkill> entry in skillTargets)
        {
            entry.Value.Execute();
        }
        ResetSkillTargets();
    }

    private void RemoveSkillTarget(SkillType skill)
    {
        if (skillTargets.ContainsKey(skill))
        {
            skillTargets.Remove(skill);
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

    private void RepositionLaneChangeUI(Vector3 screenPos)
    {
        float h = Screen.height;
        float w = Screen.width;
        float x = screenPos.x - (w / 2);
        float y = screenPos.y - (h / 2);
        float s = canvas.scaleFactor;
        laneChangeUI.anchoredPosition = new Vector2(x, y) / s;
        laneChangeUI.gameObject.SetActive(true);
    }

    public void LaneChangeUp()
    {
        if (skillTargets.ContainsKey(SkillType.LaneChange))
        {
            Debug.Log("Assigned Lane Change Up...");
            skillTargets[SkillType.LaneChange].UpdateGridCoordAction(new GridCoord(0, 1));
        }
    }

    public void LaneChangeDown()
    {
        if (skillTargets.ContainsKey(SkillType.LaneChange))
        {
            Debug.Log("Assigned Lane Change Down...");
            skillTargets[SkillType.LaneChange].UpdateGridCoordAction(new GridCoord(0, -1));
        }
    }

    public void ResetSkillBar()
    {
        for (int i = 0; i < skillToggles.Length; i++)
        {
            skillToggles[i].isOn = false;
            skillToggles[i].GetComponentInChildren<Outline>().enabled = false;
        }
        laneChangeUI.gameObject.SetActive(false);
    }

    private void CheckLaneChangeFeasibility()
    {
        return;
    }
}