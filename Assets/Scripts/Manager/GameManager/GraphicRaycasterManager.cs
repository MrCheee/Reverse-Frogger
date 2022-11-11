using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GraphicRaycasterManager : MonoBehaviour
{
    private GraphicRaycaster m_Raycaster;
    PointerEventData m_PointerEventData;
    EventSystem m_EventSystem;
    List<RaycastResult> m_TargetResults;

    private void Start()
    {
        m_Raycaster = GetComponent<GraphicRaycaster>();
        m_EventSystem = GameObject.Find("EventSystem").GetComponent<EventSystem>();
        m_TargetResults = new List<RaycastResult>();
    }

    public bool IsSelectingUI()
    {
        m_PointerEventData = new PointerEventData(m_EventSystem);
        m_PointerEventData.position = Input.mousePosition;

        m_TargetResults.Clear();
        m_Raycaster.Raycast(m_PointerEventData, m_TargetResults);

        return m_TargetResults.Count > 0 && !m_TargetResults.All(x => x.gameObject.CompareTag("Locator"));
    }

    public bool HasSelectedLocatorUI()
    {
        return m_TargetResults.Any(x => x.gameObject.CompareTag("Locator"));
    }

    public bool HasSelectedValidLocatorUI()
    {
        bool invalidLocatorSelected = m_TargetResults.Where(x => x.gameObject.CompareTag("Locator")).Any(x => x.gameObject.name == "InvalidGridSelection" || x.gameObject.name == "InvalidAirdropGridSelection");
        return !invalidLocatorSelected;
    }

    public GameObject GetSelectedLaneChangeUI()
    {
        var laneChangeElements = m_TargetResults.Where(x => x.gameObject.CompareTag("LaneChange"));
        if (laneChangeElements.Count() > 0)
        {
            return laneChangeElements.First().gameObject;
        }
        else
        {
            return null;
        }
    }
}