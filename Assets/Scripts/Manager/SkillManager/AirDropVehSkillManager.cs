using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AirDropVehSkillManager : ISkillManager
{
    public SkillType m_SkillType { get; set; }
    public ISkill m_Skill { get; set; }
    public int m_SkillCost { get; set; }
    public bool m_LockedIn { get; set; }

    private Camera GameCamera;
    private GraphicRaycasterManager graphicRaycasterManager;
    private UIMain uiMain;
    private Image invalidVehicleDropSelectionImg;
    private Image validVehicleDropSelectionImg;

    GridCoord currentVehicleSkillHoverGrid;

    GameObject SkillMarker;

    public AirDropVehSkillManager()
    {
        m_SkillType = SkillType.AirDropVeh;
        m_Skill = null;
        m_SkillCost = 10;
        m_LockedIn = false;

        currentVehicleSkillHoverGrid = new GridCoord(0, 0);

        GameCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        graphicRaycasterManager = GameObject.Find("Canvas").GetComponent<GraphicRaycasterManager>();
        uiMain = UIMain.Instance;
        invalidVehicleDropSelectionImg = GameObject.Find("InvalidGridSelection").GetComponent<Image>();
        validVehicleDropSelectionImg = GameObject.Find("ValidGridSelection").GetComponent<Image>();

        SkillMarker = GameObject.Find("AirdropSkillMarker");
    }

    public void InitialiseSkill(Unit unit)
    {
        m_Skill = new AirDropVehicle(unit);
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
        if (m_Skill != null)
        {
            GridCoord currentTargetedGrid = FieldGrid.GetGridCoordFromWorldPosition(GameCamera.ScreenToWorldPoint(Input.mousePosition));
            if (Helper.IsWithinPlayableX(currentTargetedGrid.x) && !Helper.IsEqualGridCoords(currentVehicleSkillHoverGrid, currentTargetedGrid))
            {
                Vector3 mouseOverGridPos = FieldGrid.GetSingleGrid(currentTargetedGrid).GetGridCentrePoint();
                if (!Helper.IsTargetedGridInALane(currentTargetedGrid.y) || HaveVehiclesBlockingAirDrop(currentTargetedGrid, m_Skill.unit))
                {
                    SetValidLocator(mouseOverGridPos);
                }
                else
                {
                    SetInvalidLocator(mouseOverGridPos);
                    
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
            Debug.Log("[AirDropVeh] In locator mode...");
            if (graphicRaycasterManager.HasSelectedValidLocatorUI())
            {
                Debug.Log("[AirDropVeh] Has selected valid grid...");
                GridCoord currentTargetedGrid = FieldGrid.GetGridCoordFromWorldPosition(GameCamera.ScreenToWorldPoint(Input.mousePosition));
                m_Skill.UpdateGridCoordAction(currentTargetedGrid);
                Debug.Log($"[AirDropVeh] Ready for execution, target at ({currentTargetedGrid.x}, {currentTargetedGrid.y})");
                m_LockedIn = true;
                completedSkill = true;
                SkillMarker.transform.position = FieldGrid.GetSingleGrid(currentTargetedGrid).GetGridCentrePoint();
                SkillMarker.SetActive(true);
            }
            else
            {
                Debug.Log("[AirDropVeh] Has selected invalid grid...");
                completedSkill = false;
            }
        }
        return completedSkill;
    }

    public bool CancelSelection(Unit selectedUnit)
    {
        return m_Skill == null;
    }

    private void SetValidLocator(Vector3 targetPos)
    {
        invalidVehicleDropSelectionImg.transform.position = GameCamera.WorldToScreenPoint(targetPos);
        invalidVehicleDropSelectionImg.gameObject.SetActive(true);
        validVehicleDropSelectionImg.gameObject.SetActive(false);
    }

    private void SetInvalidLocator(Vector3 targetPos)
    {
        validVehicleDropSelectionImg.transform.position = GameCamera.WorldToScreenPoint(targetPos);
        validVehicleDropSelectionImg.gameObject.SetActive(true);
        invalidVehicleDropSelectionImg.gameObject.SetActive(false);
    }

    private bool HaveVehiclesBlockingAirDrop(GridCoord targetGrid, Unit unit)
    {
        int vehicleSize = unit.GetSize();
        int direction = targetGrid.y < FieldGrid.GetDividerLaneNum() ? 1 : -1;
        bool blocked = false;

        for (int i = 0; i < vehicleSize; i++)
        {
            GridCoord checkGrid = Helper.AddGridCoords(targetGrid, new GridCoord(i * direction, 0));
            if (FieldGrid.IsWithinPlayableField(checkGrid))
            {
                List<string> gameObjectsTagInGrid = FieldGrid.GetSingleGrid(checkGrid).GetListOfUnitsGameObjectTag();
                if (gameObjectsTagInGrid.Contains("Vehicle"))
                {
                    blocked = true;
                    break;
                }
            }
        }
        return blocked;
    }

    public void DeactivateSkillUI()
    {
        uiMain.DeactivateVehicleSelectionUI();
        validVehicleDropSelectionImg.gameObject.SetActive(false);
        invalidVehicleDropSelectionImg.gameObject.SetActive(false);
    }

    public void RemoveSkillTarget()
    {
        m_Skill = null;
        m_LockedIn = false;
        validVehicleDropSelectionImg.gameObject.SetActive(false);
        SkillMarker.SetActive(false);
    }

    public void ActivateSkillUI()
    {
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
            m_Skill.Execute();
            RemoveSkillTarget();
            SkillMarker.SetActive(false);
        }
    }
}