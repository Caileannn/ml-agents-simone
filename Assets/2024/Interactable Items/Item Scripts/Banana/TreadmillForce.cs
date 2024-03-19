using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEngine;

public class TreadmillForce : MonoBehaviour
{
    public Rigidbody m_Body;
    private float m_Force;
    public float m_MaxForce;
    private float m_angleOffset;

    private void OnEnable()
    {
        OnChairContact.ApplyTreadmillForce += ApplyForce;
        OnChairContact.RemoveTreadmillForce += RemoveForce;
        m_Force = 0f;
        m_angleOffset = 20f;
    }

    private void OnDisable()
    {
        OnChairContact.ApplyTreadmillForce -= ApplyForce;
        OnChairContact.RemoveTreadmillForce -= RemoveForce;
    }

    public void ApplyForce(Rigidbody rb)
    {
        //m_Force += 0.1f;
        
        Vector3 forceDirection = Quaternion.Euler(0, -m_angleOffset, 0) * Vector3.forward;
        Debug.Log($"Force added to {rb.name} {forceDirection * m_MaxForce}");
        rb.AddForce(forceDirection * m_MaxForce);
        //m_Body.AddForce(forceDirection * m_Force);
    }

    public void RemoveForce(Rigidbody rb)
    {
        rb.AddForce(new Vector3(0, 0, 0));
    }

    private void FixedUpdate()
    {
        //Debug.Log($"Applying Force {m_Force}");
        //m_Force = (m_Force < 0f) ? 0 : (m_Force > m_MaxForce) ? m_MaxForce : m_Force;
    }

    private void OnDrawGizmosSelected()
    {
        // Draw a line representing the force direction
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, Quaternion.Euler(0, -m_angleOffset, 0) * Vector3.forward);
    }
}