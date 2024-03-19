using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteInEditMode]
public class WaypointManager : MonoBehaviour
{
    

    [SerializeField]
    public List<Waypoint> m_WaypointList;

    [SerializeField]
    public Transform m_Target;

    [SerializeField]
    public Waypoint m_CurrentWaypoint;

    private void OnEnable()
    {
        m_WaypointList = new List<Waypoint>();
        Waypoint.OnWaypointReached += SetNextWaypoint;
    }

    private void OnDisable()
    {
        Waypoint.OnWaypointReached -= SetNextWaypoint;
    }

    private void Start()
    {
        SortWaypointsInChildren();
        m_CurrentWaypoint = m_WaypointList[0];
        MoveTargetToWaypoint();
    }

    private void FixedUpdate()
    {
        MoveTargetToWaypoint();
    }

    private void MoveTargetToWaypoint()
    {
        Vector3 wpPosition = m_CurrentWaypoint.m_WaypointPosition.position;
        m_Target.position = wpPosition;
    }

    private void SetNextWaypoint(float index)
    {
        if(index == m_CurrentWaypoint.m_Index)
        {
            Debug.Log($"Waypoint reached {index}");
            MoveToNextWaypoint(index);
        }
        else
        {
            Debug.Log($"Wrong Waypoint reached {index}");
        }
    }

    private void MoveToNextWaypoint(float index)
    {
        StartCoroutine(WaypointTimeout(m_CurrentWaypoint.m_WaitTime, index));     
    }

    IEnumerator WaypointTimeout(float seconds, float index)
    {
        yield return new WaitForSeconds(seconds);
        if(m_CurrentWaypoint.m_Index < m_WaypointList.Count - 1)
        {
            m_CurrentWaypoint = m_WaypointList[(int)index + 1];
            MoveTargetToWaypoint();
            //Debug.Log($"Next waypoint {m_CurrentWaypoint.m_Index}");
        }
        else
        {
            Debug.Log($"Reached Last Position {m_CurrentWaypoint.m_Index}");
        }
    }

    private void SortWaypointsInChildren()
    {
        foreach(Transform t in transform)
        {
            m_WaypointList.Add(t.GetComponent<Waypoint>());
        }

        m_WaypointList = m_WaypointList.OrderBy(waypoint => waypoint.m_Index).ToList();
    }

    void OnDrawGizmos()
    {
#if UNITY_EDITOR
        Gizmos.color = Color.red;
        foreach(Waypoint waypoint in m_WaypointList)
        {
            Gizmos.DrawWireSphere(waypoint.m_WaypointPosition.position, 1f);
        }

        Gizmos.color = Color.red;
        for(int i = 0;  i < m_WaypointList.Count-1; i++)
        {
            Gizmos.DrawLine(m_WaypointList[i].m_WaypointPosition.position, m_WaypointList[i+1].m_WaypointPosition.position);
        }
#endif
    }
}
