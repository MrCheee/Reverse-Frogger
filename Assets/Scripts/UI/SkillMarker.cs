using UnityEngine;

public class SkillMarker : MonoBehaviour
{
    [SerializeField] GameObject TargetReticle;
    [SerializeField] GameObject SkillIcon;
    Animator SkillAnimator;

    private void Start()
    {
        SkillAnimator = GetComponent<Animator>();
    }

    public void PositionSkillMarkerUI(Vector3 position, int targetCount)
    {
        transform.position = position;
        SkillIcon.transform.localPosition = new Vector3(targetCount * 1.25f, 0, -0.8f);
        TargetReticle.SetActive(true);
    }

    public void ExecuteSkill()
    {
        TargetReticle.SetActive(false);
        SkillAnimator.SetTrigger("Execute");
    }

    public void RemoveSkillTarget()
    {
        TargetReticle.SetActive(false);
    }
}