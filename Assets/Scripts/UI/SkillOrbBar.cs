using UnityEngine;

public class SkillOrbBar : MonoBehaviour
{
    public RectTransform ActiveSkillOrbTransform;
    public RectTransform InactiveSkillOrbTransform;
    public ContentEntry ActiveSkillOrbPrefab;
    public ContentEntry InactiveSkillOrbPrefab;

    int maxOrbCount = 10;
    int currentOrbCount = 0;
    int inactiveOrbCount = 0;

    private void Awake()
    {
        for (int i = 0; i < maxOrbCount; i++)
        {
            var activeOrb = Instantiate(ActiveSkillOrbPrefab, ActiveSkillOrbTransform);
            activeOrb.gameObject.SetActive(false);
            var inactiveOrb = Instantiate(InactiveSkillOrbPrefab, InactiveSkillOrbTransform);
            inactiveOrb.gameObject.SetActive(false);
        }
    }

    public void AddSkillOrb(int count)
    {
        int orbsToAdd = Mathf.Min(maxOrbCount - currentOrbCount, count);
        for (int i = 0; i < orbsToAdd; i++)
        {
            ActiveSkillOrbTransform.GetChild(currentOrbCount + i).gameObject.SetActive(true);
            InactiveSkillOrbTransform.GetChild(currentOrbCount + i).gameObject.SetActive(true);
        }
        currentOrbCount += orbsToAdd;
    }

    //public void AddSkillOrb(int count)
    //{
    //    currentOrbCount = Mathf.Min(maxOrbCount, currentOrbCount + count);
    //    for (int i = 0; i < count; i++)
    //    {
    //        if (SkillOrbTransform.childCount < maxOrbCount)
    //        {
    //            Instantiate(ActiveSkillOrbPrefab, SkillOrbTransform);
    //        }
    //    }
    //}

    public void RemoveSkillOrb(int count)
    {
        int orbsToRemove = Mathf.Min(currentOrbCount - 0, count);
        for (int i = 0; i < orbsToRemove; i++)
        {
            ActiveSkillOrbTransform.GetChild(currentOrbCount - i - 1).gameObject.SetActive(false);
            InactiveSkillOrbTransform.GetChild(currentOrbCount - i - 1).gameObject.SetActive(false);
        }
        currentOrbCount -= orbsToRemove;
    }

    //public void RemoveSkillOrb(int count)
    //{
    //    currentOrbCount = Mathf.Max(0, currentOrbCount - count);
    //    int currentOrb = ActiveSkillOrbTransform.childCount;
    //    int orbsToRemove = Mathf.Min(currentOrb, count);
    //    for (int i = 0; i < orbsToRemove; i++)
    //    {
    //        Destroy(ActiveSkillOrbTransform.GetChild(ActiveSkillOrbTransform.childCount - 1 - i).gameObject);
    //    }
    //}
    
    public void DeactivateSkillOrb(int count)
    {
        inactiveOrbCount += count;
        RefreshSkillBar();
    }

    public void ReactivateSkillOrb(int count)
    {
        inactiveOrbCount -= count;
        if (inactiveOrbCount < 0)
        {
            Debug.Log("INACTIVE ORB COUNT REACHED BELOW ZERO, ERROR!!!");
            inactiveOrbCount = 0;
        }
        RefreshSkillBar();
    }

    public void FullRefreshSkillbar()
    {
        inactiveOrbCount = 0;
        for (int i = 0; i < maxOrbCount; i++)
        {
            //InactiveSkillOrbTransform.GetChild(i).gameObject.SetActive(false);
            if (i < currentOrbCount)
            {
                ActiveSkillOrbTransform.GetChild(i).gameObject.SetActive(true);
                InactiveSkillOrbTransform.GetChild(i).gameObject.SetActive(true);
            }
            else
            {
                ActiveSkillOrbTransform.GetChild(i).gameObject.SetActive(false);
                InactiveSkillOrbTransform.GetChild(i).gameObject.SetActive(false);
            }
        }
    }

    //public void FullRefreshSkillbar()
    //{
    //    inactiveOrbCount = 0;
    //    foreach (Transform child in ActiveSkillOrbTransform)
    //    {
    //        Destroy(child.gameObject);
    //    }
    //    for (int i = 0; i < currentOrbCount; i++)
    //    {
    //        Instantiate(ActiveSkillOrbPrefab, ActiveSkillOrbTransform);
    //    }
    //}

    public void RefreshSkillBar()
    {
        Debug.Log($"[SkillOrbBar] Current skill orbs: {currentOrbCount}. Inactive: {inactiveOrbCount}");
        int activeOrbCount = currentOrbCount - inactiveOrbCount;
        for (int i = 0; i < currentOrbCount; i++)
        {
            //InactiveSkillOrbTransform.GetChild(i).gameObject.SetActive(true);
            if (i < activeOrbCount)
            {
                ActiveSkillOrbTransform.GetChild(i).gameObject.SetActive(true);
            }
            else
            {
                ActiveSkillOrbTransform.GetChild(i).gameObject.SetActive(false);
            }
        }
    }

    //public void RefreshSkillBar()
    //{
    //    foreach (Transform child in ActiveSkillOrbTransform)
    //    {
    //        Destroy(child.gameObject);
    //    }
    //
    //    int activeOrbCount = currentOrbCount - inactiveOrbCount;
    //    for (int i = 0; i < activeOrbCount; i++)
    //    {
    //        Instantiate(ActiveSkillOrbPrefab, ActiveSkillOrbTransform);
    //    }
    //    for (int i = 0; i < inactiveOrbCount; i++)
    //    {
    //        Instantiate(InactiveSkillOrbPrefab, ActiveSkillOrbTransform);
    //    }
    //}
}