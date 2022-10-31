﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CallInVehSkillManager : ISkillManager
{
    public SkillType m_SkillType { get; set; }
    public ISkill m_Skill { get; set; }
    public int m_SkillCost { get; set; }
    public bool m_LockedIn { get; set; }

    private Camera GameCamera;
    private GraphicRaycasterManager graphicRaycasterManager;
    private UIMain uiMain;
    private RectTransform invalidVehicleLaneSelectionImg;
    private RectTransform validVehicleLaneSelectionImg;

    private List<Unit> calledInVehicles;
    GridCoord currentVehicleSkillHoverGrid;

    GameObject SkillMarker;

    public CallInVehSkillManager()
    {
        m_SkillType = SkillType.CallInVeh;
        m_Skill = null;
        m_SkillCost = 3;
        m_LockedIn = false;

        currentVehicleSkillHoverGrid = new GridCoord(0, 0);
        calledInVehicles = new List<Unit>();

        GameCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        graphicRaycasterManager = GameObject.Find("Canvas").GetComponent<GraphicRaycasterManager>();
        uiMain = UIMain.Instance;
        invalidVehicleLaneSelectionImg = GameObject.Find("InvalidLaneSelection").GetComponent<RectTransform>();
        validVehicleLaneSelectionImg = GameObject.Find("ValidLaneSelection").GetComponent<RectTransform>();

        SkillMarker = GameObject.Find("CallinVehSkillMarker");
    }

    public void InitialiseSkill(Unit unit)
    {
        m_Skill = new CallInVehicle(unit);
    }

    public void UpdateSkillUnit(Unit unit)
    {
        Debug.Log($"[CallInVeh - UpdateSkillUnit] Assigning unit {unit}");
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
        if (m_Skill != null)
        {
            GridCoord currentTargetedGrid = FieldGrid.GetGridCoordFromWorldPosition(GameCamera.ScreenToWorldPoint(Input.mousePosition));
            if (Helper.IsWithinPlayableX(currentTargetedGrid.x) && IsANewLaneHoveredOver(currentTargetedGrid.y))
            {
                GridCoord laneCentreGrid = new GridCoord(FieldGrid.GetMaxLength() / 2, currentTargetedGrid.y);
                Vector3 laneCentreGridPos = FieldGrid.GetSingleGrid(laneCentreGrid).GetGridCentrePoint();
                if (Helper.IsTargetedGridInALane(currentTargetedGrid.y) && CheckNoExistingCalledInVehInSameLane(currentTargetedGrid.y))
                {
                    SetValidLocator(laneCentreGridPos);
                }
                else
                {
                    SetInvalidLocator(laneCentreGridPos);
                }
                currentVehicleSkillHoverGrid = currentTargetedGrid;
            }
        }
    }

    public bool ProcessSelectionAndCompletedSkill(Unit selectedUnit)
    {
        bool completedSkill = false;
        if (m_Skill != null)
        {
            Debug.Log("[CallInVeh] In locator mode...");
            if (graphicRaycasterManager.HasSelectedValidLocatorUI())
            {
                Debug.Log("[CallInVeh] Has selected valid grid...");
                GridCoord currentTargetedGrid = FieldGrid.GetGridCoordFromWorldPosition(GameCamera.ScreenToWorldPoint(Input.mousePosition));
                if (currentTargetedGrid.y < FieldGrid.GetDividerLaneNum())
                {
                    currentTargetedGrid.x = FieldGrid.GetMaxLength() - FieldGrid.GetFieldBuffer();
                }
                else
                {
                    currentTargetedGrid.x = FieldGrid.GetFieldBuffer() - 1;
                }
                m_Skill.UpdateGridCoordAction(currentTargetedGrid);
                Debug.Log($"[CallInVeh] Ready for execution, target at ({currentTargetedGrid.x}, {currentTargetedGrid.y})");
                m_LockedIn = true;
                completedSkill = true;

                GridCoord laneCentreGrid = new GridCoord(FieldGrid.GetMaxLength() / 2, currentTargetedGrid.y);
                SkillMarker.transform.position = FieldGrid.GetSingleGrid(laneCentreGrid).GetGridCentrePoint();
                SkillMarker.SetActive(true);
            }
            else
            {
                Debug.Log("[CallInVeh] Has selected invalid grid...");
                completedSkill = false;
            }
        }
        return completedSkill;
    }

    public bool CancelSelection(Unit selectedUnit)
    {
        return m_Skill == null;
    }

    private bool IsANewLaneHoveredOver(int gridY)
    {
        return currentVehicleSkillHoverGrid.y != gridY;
    }

    private void SetValidLocator(Vector3 targetPos)
    {
        validVehicleLaneSelectionImg.transform.position = GameCamera.WorldToScreenPoint(targetPos);
        validVehicleLaneSelectionImg.gameObject.SetActive(true);
        invalidVehicleLaneSelectionImg.gameObject.SetActive(false);
    }

    private void SetInvalidLocator(Vector3 targetPos)
    {
        invalidVehicleLaneSelectionImg.transform.position = GameCamera.WorldToScreenPoint(targetPos);
        invalidVehicleLaneSelectionImg.gameObject.SetActive(true);
        validVehicleLaneSelectionImg.gameObject.SetActive(false);
    }

    private bool CheckNoExistingCalledInVehInSameLane(int gridY)
    {
        return calledInVehicles.All(x => x.GetCurrentHeadGridPosition().y != gridY);
    }

    public void DeactivateSkillUI()
    {
        uiMain.DeactivateVehicleSelectionUI();
        validVehicleLaneSelectionImg.gameObject.SetActive(false);
        invalidVehicleLaneSelectionImg.gameObject.SetActive(false);
    }

    public void RemoveSkillTarget()
    {
        m_Skill = null;
        m_LockedIn = false;
        validVehicleLaneSelectionImg.gameObject.SetActive(false);
        SkillMarker.SetActive(false);
    }

    public void ActivateSkillUI()
    {
        calledInVehicles = calledInVehicles.Where(x => x != null && !Helper.IsWithinPlayableX(x.GetCurrentHeadGridPosition().x)).ToList();
        uiMain.ActivateVehicleSelectionUI();
    }

    public void ProcessSelection(Unit selectedUnit)
    {
        return;
    }

    public void ExecuteSkill()
    {
        if (m_LockedIn)
        {
            calledInVehicles.Add(m_Skill.unit);
            Debug.Log(m_Skill.unit);
            m_Skill.Execute();
            RemoveSkillTarget();
            SkillMarker.SetActive(false);
        }
    }
}
