using UnityEngine;

public class SnipeSkillManager : ISkillManager
{
    public SkillType m_SkillType { get; set; }
    public ISkill m_Skill { get; set; }
    public int m_SkillCost { get; set; }
    public bool m_LockedIn { get; set; }

    GameObject SkillMarker;
    GameObject SkillIcon;

    public SnipeSkillManager()
    {
        m_SkillType = SkillType.Assassinate;
        m_Skill = null;
        m_SkillCost = 8;
        m_LockedIn = false;
        SkillMarker = GameObject.Find("SnipeTarget");
        SkillIcon = GameObject.Find("SnipeIcon");
    }

    public void InitialiseSkill(Unit unit)
    {
        m_Skill = new Snipe(unit);
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
        // Snipe only works on enemy units
        if (selectedUnit != null && selectedUnit.gameObject.tag == "Enemy")
        {
            // Snipe cannot be used on enemies on top of vehicles (dangerous thematically)
            if (selectedUnit.GetComponent<Unit>().yAdjustment != 3)
            {
                m_LockedIn = true;
                selectedUnit.AddTargetedSkill(m_SkillType);
                UpdateSkillUnit(selectedUnit);
                PositionSkillMarkerUI();
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    public bool CancelSelection(Unit selectedUnit)
    {
        return false;
    }

    public void ActivateSkillUI()
    {
        return;
    }

    public void DeactivateSkillUI()
    {
        return;
    }

    public void RemoveSkillTarget()
    {
        if (m_Skill != null && m_Skill.unit != null)
        {
            m_Skill.unit.RemoveTargetedSkill(m_SkillType);
        }
        m_Skill = null;
        m_LockedIn = false;
        SkillMarker.SetActive(false);
    }

    public void ProcessSelection(Unit selectedUnit)
    {
        return;
    }

    public void ExecuteSkill()
    {
        if (m_LockedIn)
        {
            m_Skill.Execute();
            SkillMarker.SetActive(false);
        }
    }

    public string GetExecuteLog()
    {
        Unit targetUnit = m_Skill.unit.GetComponent<Unit>();
        return $"Snipe Skill used on {targetUnit.GetName()} at Grid [{targetUnit.GetCurrentHeadGridPosition().x}, {targetUnit.GetCurrentHeadGridPosition().y}].";
    }

    public void PositionSkillMarkerUI()
    {
        if (m_Skill != null)
        {
            SkillMarker.transform.position = m_Skill.unit.transform.position;
            SkillIcon.transform.localPosition = new Vector3(m_Skill.unit.GetTargetedCount() * 1.25f, 0, -0.8f);
            SkillMarker.SetActive(true);
        }
    }
}
