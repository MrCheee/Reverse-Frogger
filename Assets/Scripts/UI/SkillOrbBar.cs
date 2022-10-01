﻿using UnityEngine;

public class SkillOrbBar : MonoBehaviour
{
    public RectTransform SkillOrbTransform;
    public ContentEntry ActiveSkillOrbPrefab;
    public ContentEntry InactiveSkillOrbPrefab;

    int maxOrbCount = 10;
    int totalOrbCount = 0;
    int inactiveOrbCount = 0;

    public void AddSkillOrb(int count)
    {
        totalOrbCount = Mathf.Min(maxOrbCount, totalOrbCount + count);
        for (int i = 0; i < count; i++)
        {
            if (SkillOrbTransform.childCount < maxOrbCount)
            {
                Instantiate(ActiveSkillOrbPrefab, SkillOrbTransform);
            }
        }
    }

    public void RemoveSkillOrb(int count)
    {
        totalOrbCount = Mathf.Max(0, totalOrbCount - count);
        int currentOrb = SkillOrbTransform.childCount;
        int orbsToRemove = Mathf.Min(currentOrb, count);
        for (int i = 0; i < orbsToRemove; i++)
        {
            Destroy(SkillOrbTransform.GetChild(SkillOrbTransform.childCount - 1 - i).gameObject);
        }
    }

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
        foreach (Transform child in SkillOrbTransform)
        {
            Destroy(child.gameObject);
        }
        for (int i = 0; i < totalOrbCount; i++)
        {
            Instantiate(ActiveSkillOrbPrefab, SkillOrbTransform);
        }
    }

    public void RefreshSkillBar()
    {
        foreach (Transform child in SkillOrbTransform)
        {
            Destroy(child.gameObject);
        }

        int activeOrbCount = totalOrbCount - inactiveOrbCount;
        for (int i = 0; i < activeOrbCount; i++)
        {
            Instantiate(ActiveSkillOrbPrefab, SkillOrbTransform);
        }
        for (int i = 0; i < inactiveOrbCount; i++)
        {
            Instantiate(InactiveSkillOrbPrefab, SkillOrbTransform);
        }
    }
}