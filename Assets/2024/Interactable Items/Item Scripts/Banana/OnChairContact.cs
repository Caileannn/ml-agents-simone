using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgentsExamples;
using UnityEngine;
using UnityEngine.Events;

public class OnChairContact : MonoBehaviour
{
    public UnityEvent m_CollisionEvent;
    public UnityEvent m_CollisionStayEvent;
    public UnityEvent m_CollisionExitEvent;

    public UnityEvent m_TriggerStayEvent;
    public UnityEvent m_TriggerExitEvent;

    public bool m_IsTouching;
    public bool m_IsAttached;
    public bool m_IsTreadmill;
    public float m_TrayTimeout;
    private string m_Tag = "agent";

    [SerializeField]
    public FixedJoint m_FixedJoint;

    public delegate void TreadmillEventApply(Rigidbody rb);

    public static event TreadmillEventApply ApplyTreadmillForce;

    public delegate void TreadmillEventRemove(Rigidbody rb);

    public static event TreadmillEventRemove RemoveTreadmillForce;


    void OnEnable()
    {
        m_IsTouching = false;
        m_IsAttached = false;
    }

    private void Start()
    {
        if (this.TryGetComponent<FixedJoint>(out FixedJoint component))
        {
            m_FixedJoint = component;
        }
        else
        {
            m_FixedJoint = null;
        }
    }

    private void OnCollisionEnter(Collision col)
    {
        if (col.transform.CompareTag(m_Tag))
        {
            if (col.gameObject.GetComponent<GroundContact>().isFoot && m_FixedJoint && !m_IsAttached)
            {
                m_IsAttached = true;
                // Attach Joint
                Rigidbody rbFoot = col.gameObject.GetComponent<Rigidbody>();
                Debug.Log($"Attached to {col.gameObject.name}");
                m_FixedJoint.connectedBody = rbFoot;
                StartCoroutine(WaitToDetatch());
            }

            m_IsTouching = true;
            m_CollisionEvent?.Invoke();
            
            
        }
    }

    private void OnCollisionStay(Collision col)
    {
        if (col.transform.CompareTag(m_Tag))
        {
            m_IsTouching = true;
            m_CollisionStayEvent?.Invoke();
        }
    }

    private void OnCollisionExit(Collision other)
    {
        if (other.transform.CompareTag(m_Tag))
        {
            m_IsTouching = false;
            m_CollisionExitEvent?.Invoke();

            
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.transform.CompareTag(m_Tag))
        {
            m_IsTouching = true;
            m_TriggerStayEvent?.Invoke();

            if (m_IsTreadmill)
            {
                ApplyTreadmillForce(other.GetComponent<Rigidbody>());
            }
        }


    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.CompareTag(m_Tag))
        {
            m_IsTouching = false;
            m_TriggerExitEvent?.Invoke();
            if (m_IsTreadmill)
            {
                RemoveTreadmillForce(other.GetComponent<Rigidbody>());
            }
        }
    }

    IEnumerator WaitToDetatch()
    {
        Debug.Log("Waiting for detatch");
        yield return new WaitForSeconds(m_TrayTimeout);
        Destroy(m_FixedJoint);
    }
}
