using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorManager : MonoBehaviour
{
    public PhysicMaterial m_Floor;
    public PhysicMaterial m_Agent;
    public float m_Timer;
    public float m_TimerTray;
    public bool m_HasChanged;

    // Start is called before the first frame update
    private void OnEnable()
    {
        m_Timer = 5f;
        m_HasChanged = false;
        m_Floor.staticFriction = 1000f;
        m_Floor.dynamicFriction = 1000f;
        m_Agent.staticFriction = 2f;
        m_Agent.dynamicFriction = 2f;
    }

    void Start()
    {
        if (m_Floor == null || m_Agent == null)
            Debug.Log($"Physics Material of Floor/Agent was never set in FloorManager");
    }

    public void ActivateSlipperySurface()
    {
        if(!m_HasChanged)
            Debug.Log($"Starting Cortouine on Banana for {m_Timer} seconds.");
            StartCoroutine(ChangeSurface());
    }

    public void ActivateSlipperyChair()
    {
        if (!m_HasChanged)
            Debug.Log($"Starting Cortouine on Chair for {m_Timer} seconds.");
            StartCoroutine(ChangeChair());
    }

    public void ActivateSlipperyTray()
    {
        if (!m_HasChanged)
            Debug.Log($"Starting Cortouine on Tray for {m_TimerTray} seconds.");
            StartCoroutine(IkeaTrayAttach());
    }

    IEnumerator ChangeSurface()
    {
        
        m_HasChanged = true;

        m_Floor.staticFriction = 0f;
        m_Floor.dynamicFriction = 0f;

        m_Agent.staticFriction = 0f;
        m_Agent.dynamicFriction = 0f;

        yield return new WaitForSeconds( m_Timer );

        m_Floor.staticFriction = 1000f;
        m_Floor.dynamicFriction = 1000f;

        m_Agent.staticFriction = 2f;
        m_Agent.dynamicFriction = 2f;

        m_HasChanged = false;
        Debug.Log($"Coroutine has ended for Banana");
    }

    IEnumerator ChangeChair()
    {

        m_HasChanged = true;

        

        yield return new WaitForSeconds(m_Timer/2);

        m_Agent.staticFriction = 0f;
        m_Agent.dynamicFriction = 0f;

        yield return new WaitForSeconds(m_Timer / 2);

        m_Agent.staticFriction = 2f;
        m_Agent.dynamicFriction = 2f;

        m_HasChanged = false;
        Debug.Log($"Coroutine has ended for Banana");
    }

    IEnumerator IkeaTrayAttach()
    {
        m_HasChanged = true;

        m_Floor.staticFriction = 0f;
        m_Floor.dynamicFriction = 0f;

        m_Agent.staticFriction = 0f;
        m_Agent.dynamicFriction = 0f;

        yield return new WaitForSeconds(m_TimerTray);

        m_Floor.staticFriction = 1000f;
        m_Floor.dynamicFriction = 1000f;

        m_Agent.staticFriction = 2f;
        m_Agent.dynamicFriction = 2f;

        Debug.Log($"Coroutine has ended for Tray");
    }
}
