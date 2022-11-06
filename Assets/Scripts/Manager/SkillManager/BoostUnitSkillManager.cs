using UnityEngine;

public class BoostUnitSkillManager : ISkillManager
{
    public SkillType m_SkillType { get; set; }
    public ISkill m_Skill { get; set; }
    public int m_SkillCost { get; set; }
    public bool m_LockedIn { get; set; }

    GameObject SkillMarker;

    public BoostUnitSkillManager()
    {
        SkillMarker = GameObject.Find("BoostTarget");
        m_SkillType = SkillType.BoostUnit;
        m_Skill = null;
        m_SkillCost = 3;
        m_LockedIn = false;
    }

    public void InitialiseSkill(Unit unit)
    {
        m_Skill = new BoostUnit(unit);
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
        if (selectedUnit != null)
        {
            UpdateSkillUnit(selectedUnit);
            m_LockedIn = true;
            SkillMarker.transform.position = selectedUnit.transform.position;
            SkillMarker.SetActive(true);
            return true;
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
        return $"Boost Unit Skill used on {targetUnit.GetName()} at Grid [{targetUnit.GetCurrentHeadGridPosition().x}, {targetUnit.GetCurrentHeadGridPosition().y}].";
    }
}