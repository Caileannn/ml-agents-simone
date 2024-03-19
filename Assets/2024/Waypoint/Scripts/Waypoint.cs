using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[ExecuteInEditMode]
public class Waypoint : MonoBehaviour {


    [HideInInspector]
    public Transform m_WaypointPosition;

    public float m_WaitTime;

    public string m_ModelName;

    public float m_Index;

    public bool m_Touched;

    public delegate void WaypointReached(float index);

    public static event WaypointReached OnWaypointReached;

    public delegate void SwapModelOnReachingWaypoint(string modelName);

    public static event SwapModelOnReachingWaypoint SwapModelOnWaypointReached;

    private void OnEnable()
    {
        m_WaypointPosition = this.transform;
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"{other.name}");
        if (other.transform.CompareTag("agent") && !m_Touched)
        {
            if (OnWaypointReached != null)
            {
                m_Touched = true;
                OnWaypointReached(m_Index);

                if(!string.IsNullOrEmpty(m_ModelName))
                {
                    SwapModelOnWaypointReached(m_ModelName);
                }
                
            }
                
        }
    }
}
