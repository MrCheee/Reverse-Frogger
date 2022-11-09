using UnityEngine;

public class DisableUnitSkillManager : ISkillManager
{
    public SkillType m_SkillType { get; set; }
    public ISkill m_Skill { get; set; }
    public int m_SkillCost { get; set; }
    public bool m_LockedIn { get; set; }

    SkillMarker m_SkillMarker;

    public DisableUnitSkillManager()
    {
        m_SkillType = SkillType.DisableUnit;
        m_Skill = null;
        m_SkillCost = 3;
        m_LockedIn = false;
        m_SkillMarker = GameObject.Find("DisableTarget").GetComponent<SkillMarker>();
    }

    public void InitialiseSkill(Unit unit)
    {
        m_Skill = new DisableUnit(unit);
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
            m_LockedIn = true;
            selectedUnit.AddTargetedSkill(m_SkillType);
            UpdateSkillUnit(selectedUnit);
            m_SkillMarker.PositionSkillMarkerUI(selectedUnit.transform.position, selectedUnit.GetTargetedCount());
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
        if (m_Skill != null && m_Skill.unit != null)
        {
            m_Skill.unit.RemoveTargetedSkill(m_SkillType);
        }
        m_Skill = null;
        m_LockedIn = false;
        m_SkillMarker.RemoveSkillTarget();
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
            m_SkillMarker.ExecuteSkill();
        }
    }

    public string GetExecuteLog()
    {
        Unit targetUnit = m_Skill.unit.GetComponent<Unit>();
        return $"Disable Unit Skill used on {targetUnit.GetName()} at Grid [{targetUnit.GetCurrentHeadGridPosition().x}, {targetUnit.GetCurrentHeadGridPosition().y}].";
    }

    public void RepositionSkillMarkerUI()
    {
        if (m_Skill != null && m_Skill.unit != null)
        {
            m_SkillMarker.PositionSkillMarkerUI(m_Skill.unit.transform.position, m_Skill.unit.GetTargetedCount());
        }
    }
}
