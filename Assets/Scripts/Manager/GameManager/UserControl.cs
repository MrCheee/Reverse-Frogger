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
    private GraphicRaycasterManager graphicRaycasterManager;
    private UIMain uiMain;
    private PlayerSkillVehicleManager playerSkillVehicleManager;

    [SerializeField] private Toggle[] skillToggles;
    [SerializeField] private Sprite[] skillDefaultSprites;
    [SerializeField] private Sprite[] skillSelectedSprites;
    [SerializeField] private Button[] carChoosingButtons;
    private Color skillLockedInColor;
    private Color skillDefaultColor;
    private bool[] skillLockedInStatus;
    private bool skillExecutionInProgress = false;

    private Unit m_Selected = null;
    private SkillType skillSelected;
    private Dictionary<SkillType, ISkillManager> skillManagers;

    private int currentSkillOrbCount = 0;
    private int consumedSkillOrbCount = 0;
    private int maxSkillOrbCount = 10;

    private void Awake()
    {
        uiMain = UIMain.Instance;
        playerSkillVehicleManager = gameObject.GetComponent<PlayerSkillVehicleManager>();
        graphicRaycasterManager = GameObject.Find("Canvas").GetComponent<GraphicRaycasterManager>();
        skillSelected = SkillType.None;
        Marker.SetActive(false);

        skillManagers = new Dictionary<SkillType, ISkillManager> {
            { SkillType.Assassinate, new SnipeSkillManager() },
            { SkillType.CallInVeh, new CallInVehSkillManager() },
            { SkillType.AirDropVeh, new AirDropVehSkillManager() },
            { SkillType.LaneChange, new LaneChangeSkillManager() },
            { SkillType.BoostUnit, new BoostUnitSkillManager() },
            { SkillType.DisableUnit, new DisableUnitSkillManager() }
        };

        skillLockedInStatus = new bool[6];
        for (int i = 0; i < skillToggles.Length; i++)
        {
            int tmp = i;
            skillToggles[i].onValueChanged.AddListener(delegate { HandleSelectSkill(tmp); });
            skillLockedInStatus[i] = false;
        }
        skillLockedInColor = new Color(255, 255, 0, 1f);
        skillDefaultColor = new Color(255, 255, 255, 1f);

        for (int i = 0; i < carChoosingButtons.Length; i++)
        {
            int tmp = i;
            carChoosingButtons[i].onClick.AddListener(delegate { HandleSelectCar(tmp); });
        }
    }

    private void Update()
    {
        if (skillSelected != SkillType.None)
        {
            skillManagers[skillSelected].ExecuteUpdateCheck();
        }
        
        if (Input.GetMouseButtonDown(0))
        {
            HandleSelection();
        }

        if (Input.GetMouseButtonDown(1))
        {
            HandleDeselection();
        }

        //MarkerHandling();
    }

    public void HandleSelection()
    {
        Vector3 screenPos = Input.mousePosition;
        var ray = GameCamera.ScreenPointToRay(screenPos);

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            var unit = hit.collider.GetComponentInParent<Unit>();
            m_Selected = unit;

            if (graphicRaycasterManager.IsSelectingUI())
            {
                Debug.Log("Selected UI Element, ignoring update on InfoPopup...");
            }
            else
            {
                MarkerHandling();
                var uiInfo = hit.collider.GetComponentInParent<UIMain.IUIInfoContent>();
                UIMain.Instance.SetNewInfoContent(uiInfo);
                if (skillSelected != SkillType.None && skillManagers[skillSelected].CancelSelection(m_Selected))
                {
                    skillToggles[(int)skillSelected].GetComponentInChildren<Image>().sprite = skillDefaultSprites[(int)skillSelected];
                    skillToggles[(int)skillSelected].isOn = false;
                }
            }

            // If targeting a skill, register the unit under the skill manager and the skill manager
            // will handle the skill execution when player turn ends. 
            // Cancellation of commands before turn is officially ended must be possible.
            // At the moment, each skill can only be used once, so a dictionary is used with 1 manager per skill
            if (skillSelected != SkillType.None)
            {
                if (skillManagers[skillSelected].ProcessSelectionAndCompletedSkill(m_Selected))
                {
                    // After the skill has been locked in, the button will become highlighted in green on the borders, 
                    // and it will not be yellow anymore.
                    HighlightCurrentSelectedButton();
                    ResetCurrentSelectedButton();
                    skillSelected = SkillType.None;
                }
                else
                {
                    skillManagers[skillSelected].ProcessSelection(m_Selected);
                }
            }
        }
    }
    public void HandleDeselection()
    {
        m_Selected = null;
        UIMain.Instance.SetNewInfoContent(null);
        RemoveSkillTarget(skillSelected);
        ResetCurrentSelectedButton();
        skillSelected = SkillType.None;
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

    private void HandleSelectSkill(int skillNum)
    {
        int skillCost = skillManagers[(SkillType)skillNum].m_SkillCost;
        SkillType skillType = (SkillType)skillNum;

        // If button is selected
        if (skillToggles[skillNum].isOn)
        {
            Debug.Log($"Skill {skillType} has been selected!");
            // Remove previously selected skill from active state and set this as current active skill
            ResetPreviousSelectedButton(skillType);
            SetActiveSkill(skillType);

            // If current active skill was previously assigned a target already, reselection will cancel that target
            if (IsSkillLockedIn(skillNum) == true)
            {
                uiMain.ReactivateSkillOrb(skillCost);
                consumedSkillOrbCount -= skillCost;
                ResetHighlightCurrentSelectedButton();
                RemoveSkillTarget(skillType);
            }

            // Set state of deactivated skill orbs based on selected skill
            uiMain.DeactivateSkillOrb(skillCost);
            consumedSkillOrbCount += skillCost;
        }
        else  // If button is deselected, either manually, or auto trigger when a skill target is locked in
        {
            Debug.Log($"Skill {skillType} has been deselected!");
            // If skill is not locked in, refund skill orbs
            if (IsSkillLockedIn(skillNum) == false)
            {
                uiMain.ReactivateSkillOrb(skillCost);
                consumedSkillOrbCount -= skillCost;
            }
            ClearActiveSkill();
        }
    }

    private void HandleSelectCar(int carNum)
    {
        Debug.Log("[HandleSelectCar]");
        Unit selectedVehicle = playerSkillVehicleManager.GetPlayerSkillVehicle(carNum).GetComponent<Unit>();
        skillManagers[skillSelected].UpdateSkillUnit(selectedVehicle);
        skillManagers[skillSelected].DeactivateSkillUI();
    }

    private bool IsSkillLockedIn(int skillNum)
    {
        return skillLockedInStatus[(int)skillSelected];
    }

    public void RefreshSkillsUI()
    {
        currentSkillOrbCount = Mathf.Min(maxSkillOrbCount, currentSkillOrbCount);
        uiMain.RefreshSkillOrbBar();
        ResetSkillBar();
        consumedSkillOrbCount = 0;
    }

    private void SetActiveSkill(SkillType skill)
    {
        skillSelected = skill;
        skillToggles[(int)skillSelected].GetComponentInChildren<Image>().sprite = skillSelectedSprites[(int)skillSelected];
        skillManagers[skillSelected].ActivateSkillUI();
    }

    private void ClearActiveSkill()
    {
        if (IsSkillLockedIn((int)skillSelected) == false)
        {
            skillManagers[skillSelected].DeactivateSkillUI();
        }
        skillToggles[(int)skillSelected].GetComponentInChildren<Image>().sprite = skillDefaultSprites[(int)skillSelected];
        skillSelected = SkillType.None;
    }

    private void ResetCurrentSelectedButton()
    {
        if (skillSelected != SkillType.None)
        {
            skillToggles[(int)skillSelected].GetComponentInChildren<Image>().sprite = skillDefaultSprites[(int)skillSelected];
            skillToggles[(int)skillSelected].isOn = false;
        }
    }

    public void UICancelCurrentSkillSelection()
    {
        ResetCurrentSelectedButton();
    }

    private void ResetPreviousSelectedButton(SkillType skill)
    {
        if (skillSelected != SkillType.None && skillSelected != skill)
        {
            skillToggles[(int)skillSelected].GetComponentInChildren<Image>().sprite = skillDefaultSprites[(int)skillSelected];
            skillToggles[(int)skillSelected].isOn = false;
        }
    }

    private void HighlightCurrentSelectedButton()
    {
        if (skillSelected != SkillType.None)
        {
            skillToggles[(int)skillSelected].GetComponentInChildren<Image>().sprite = skillDefaultSprites[(int)skillSelected];
            skillToggles[(int)skillSelected].GetComponentInChildren<Image>().color = skillLockedInColor;
            skillLockedInStatus[(int)skillSelected] = true;
        }
        UpdateSkillTogglesFunctionality();
    }

    private void ResetHighlightCurrentSelectedButton()
    {
        if (skillSelected != SkillType.None)
        {
            skillToggles[(int)skillSelected].GetComponentInChildren<Image>().color = skillDefaultColor;
            skillLockedInStatus[(int)skillSelected] = false;
            UpdateSkillTogglesFunctionality();
        }
    }

    public void DispatchSkillCommands()
    {
        skillExecutionInProgress = true;
        StartCoroutine("ExecuteSkills");
    }

    private IEnumerator ExecuteSkills()
    {
        //int skillOrbsConsumed = 0;
        foreach (KeyValuePair<SkillType, ISkillManager> entry in skillManagers)
        {
            if (entry.Value.m_LockedIn)
            {
                uiMain.RemoveSkillOrb(entry.Value.m_SkillCost);
                uiMain.UpdateGameLog(entry.Value.GetExecuteLog());
                entry.Value.ExecuteSkill();
                //skillOrbsConsumed += entry.Value.m_SkillCost;
                currentSkillOrbCount -= entry.Value.m_SkillCost;
                yield return new WaitForSeconds(2f);
            }
        }
        //uiMain.RemoveSkillOrb(skillOrbsConsumed);
        ResetAllSkillTargets();
        skillExecutionInProgress = false;
    }

    public bool DispatchSkillCompleted()
    {
        return !skillExecutionInProgress;
    }

    private void RemoveSkillTarget(SkillType skill)
    {
        skillManagers[skill].RemoveSkillTarget();
        foreach (ISkillManager mgr in skillManagers.Values)
        {
            mgr.PositionSkillMarkerUI();
        }
    }

    private void ResetAllSkillTargets()
    {
        foreach (KeyValuePair<SkillType, ISkillManager> entry in skillManagers)
        {
            entry.Value.RemoveSkillTarget();
        }
    }

    public void AddSkillOrb(int count)
    {
        currentSkillOrbCount += count;
        uiMain.AddSkillOrb(count);
    }

    public void ResetSkillBar()
    {
        for (int i = 0; i < skillToggles.Length; i++)
        {
            EnableSkillButton(skillToggles[i]);
            skillToggles[i].GetComponentInChildren<Image>().sprite = skillDefaultSprites[i];
            skillToggles[i].GetComponentInChildren<Image>().color = skillDefaultColor;
            skillToggles[i].isOn = false;
            skillLockedInStatus[i] = false;
        }
    }

    public void DisableSkillBar()
    {
        for (int i = 0; i < skillToggles.Length; i++)
        {
            if (skillToggles[i].isOn)
            {
                skillToggles[i].isOn = false;
            }
            DisableSkillButton(skillToggles[i]);
        }
    }

    public void UpdateSkillTogglesFunctionality()
    {
        Debug.Log($"[UserControl] Current skill orbs: {currentSkillOrbCount}. Consumed: {consumedSkillOrbCount}");
        int skillOrbCount = currentSkillOrbCount - consumedSkillOrbCount;
        for (int i = 0; i < skillToggles.Length; i++)
        {
            if (skillManagers[(SkillType)i].m_SkillCost > skillOrbCount)
            {
                if (skillToggles[i].isOn == false && !skillLockedInStatus[i])
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