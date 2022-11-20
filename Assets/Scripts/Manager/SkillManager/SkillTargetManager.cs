using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SkillTargetManager : MonoBehaviour
{
    [SerializeField] SkillMarker[] skillMarkers;  // Skill markers pre-created in the scene, one per targetable skill
    [SerializeField] Sprite[] skillIcons;
    Dictionary<SkillType, int> skillIndex;
    Unit[] targetedUnits;

    private void Start()
    {
        skillIndex = new Dictionary<SkillType, int> {
            { SkillType.Assassinate, 0 },
            { SkillType.BoostUnit, 1 },
            { SkillType.DisableUnit, 2 } 
        };
        targetedUnits = Enumerable.Repeat<Unit>(null, 3).ToArray();
    }

    public void TargetSkillOnUnit(SkillType skill, Unit unit)
    {
        int idx = skillIndex[skill];

        if (!targetedUnits.Contains(unit))
        {
            SkillMarker availableMarker = skillMarkers.Where(x => !x.gameObject.activeInHierarchy).First();
            availableMarker.PositionSkillMarkerUI(unit.transform.position, 1);
        }

        targetedUnits[idx] = unit;

    }
}