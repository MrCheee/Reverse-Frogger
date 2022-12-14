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

    private SkillSoundManager skillSoundManager;
    private Camera GameCamera;
    private GraphicRaycasterManager graphicRaycasterManager;
    private UIMain uiMain;
    private Image invalidVehicleDropSelectionImg;
    private Image validVehicleDropSelectionImg;

    GridCoord currentVehicleSkillHoverGrid;

    public AirDropVehSkillManager()
    {
        m_SkillType = SkillType.AirDropVeh;
        m_Skill = null;
        m_SkillCost = 10;
        m_LockedIn = false;

        currentVehicleSkillHoverGrid = new GridCoord(0, 0);

        skillSoundManager = GameObject.Find("SkillSoundManager").GetComponent<SkillSoundManager>();
        GameCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        graphicRaycasterManager = GameObject.Find("Canvas").GetComponent<GraphicRaycasterManager>();
        uiMain = UIMain.Instance;
        invalidVehicleDropSelectionImg = GameObject.Find("InvalidAirdropGridSelection").GetComponent<Image>();
        validVehicleDropSelectionImg = GameObject.Find("ValidAirdropGridSelection").GetComponent<Image>();
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
            m_Skill.TargetUnit = unit;
        }
    }

    public void ExecuteUpdateCheck()
    {
        if (m_Skill != null)
        {
            GridCoord currentTargetedGrid = FieldGrid.GetGridCoordFromWorldPosition(GameCamera.ScreenToWorldPoint(Input.mousePosition));
            if (FieldGrid.IsWithinPlayableX(currentTargetedGrid.x) && !Helper.IsEqualGridCoords(currentVehicleSkillHoverGrid, currentTargetedGrid))
            {
                Vector3 mouseOverGridPos = FieldGrid.GetGrid(currentTargetedGrid).GetGridCentrePoint();
                if (!FieldGrid.IsTargetedGridInALane(currentTargetedGrid.y) || HaveVehiclesBlockingAirDrop(currentTargetedGrid, m_Skill.TargetUnit))
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
            if (graphicRaycasterManager.HasSelectedLocatorUI())
            {
                Debug.Log("[AirDropVeh] In locator mode...");
                if (graphicRaycasterManager.HasSelectedValidLocatorUI())
                {
                    Debug.Log("[AirDropVeh] Has selected valid grid...");
                    skillSoundManager.PlaySkillConfirm();
                    GridCoord currentTargetedGrid = FieldGrid.GetGridCoordFromWorldPosition(GameCamera.ScreenToWorldPoint(Input.mousePosition));
                    m_Skill.UpdateGridCoordAction(currentTargetedGrid);
                    Debug.Log($"[AirDropVeh] Ready for execution, target at ({currentTargetedGrid.x}, {currentTargetedGrid.y})");
                    m_LockedIn = true;
                    completedSkill = true;
                }
                else
                {
                    Debug.Log("[AirDropVeh] Has selected invalid grid...");
                    skillSoundManager.PlayInvalidSelection();
                    completedSkill = false;
                }
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
        bool blocked = false;
        if (unit is Vehicle veh)
        {
            int direction = targetGrid.y < FieldGrid.DividerY ? 1 : -1;
            for (int i = 0; i < veh.Size; i++)
            {
                GridCoord checkGrid = Helper.AddGridCoords(targetGrid, new GridCoord(i * direction, 0));
                if (FieldGrid.IsWithinPlayableField(checkGrid))
                {
                    List<string> gameObjectsTagInGrid = FieldGrid.GetGrid(checkGrid).GetListOfUnitsGameObjectTag();
                    if (gameObjectsTagInGrid.Contains("Vehicle"))
                    {
                        blocked = true;
                        break;
                    }
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
            skillSoundManager.PlayAirdrop();
            m_Skill.Execute();
            RemoveSkillTarget();
        }
    }

    public string GetExecuteLog()
    {
        Unit targetUnit = m_Skill.TargetUnit.GetComponent<Unit>();
        return $"Air Drop Vehicle Skill used to drop a {targetUnit.GetName()} at Grid [{m_Skill.TargetGrid.x}, {m_Skill.TargetGrid.y}]";
    }

    public void RepositionSkillMarkerUI()
    {
        return;
    }
}