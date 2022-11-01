interface ISkillManager
{
    public SkillType m_SkillType { get; set; }
    public ISkill m_Skill { get; set; }
    public int m_SkillCost { get; set; }
    public bool m_LockedIn { get; set; }
    void InitialiseSkill(Unit unit);
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
}