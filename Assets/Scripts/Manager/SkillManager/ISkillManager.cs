using System;

interface ISkillManager
{
    SkillType m_SkillType { get; set; }
    ISkill m_Skill { get; set; }
    int m_SkillCost { get; set; }
    bool m_LockedIn { get; set; }
    void UpdateSkillUnit(Unit unit);
    void ExecuteUpdateCheck();
    bool ProcessSelectionAndCompletedSkill(Unit selectedUnit);
    void ProcessSelection(Unit selectedUnit);
    bool CancelSelection(Unit selectedUnit);
    void ActivateSkillUI();
    void DeactivateSkillUI();
    void RemoveSkillTarget();
    void ExecuteSkill();
    string GetExecuteLog();
    void RepositionSkillMarkerUI();
}