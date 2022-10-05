public class DisableUnitSkillManager : ISkillManager
{
    public SkillType m_SkillType { get; set; }
    public ISkill m_Skill { get; set; }
    public int m_SkillCost { get; set; }
    public bool m_LockedIn { get; set; }

    public DisableUnitSkillManager()
    {
        m_SkillType = SkillType.DisableUnit;
        m_Skill = null;
        m_SkillCost = 3;
        m_LockedIn = false;
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
            UpdateSkillUnit(selectedUnit);
            m_LockedIn = true;
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
    }

    public void ProcessSelection(Unit selectedUnit)
    {
        return;
    }

    public void ExecuteSkill()
    {
        if (m_LockedIn) m_Skill.Execute();
    }
}
