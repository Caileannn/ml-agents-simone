using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;

public enum Team
{
    Blue = 0,
    Purple = 1
}

public class AgentClass : Agent
{

    public enum Position
    {
        Generic
    }


    [HideInInspector]
    public Team team;
    float m_BallTouched;
    public Position position;

    float m_Existential;


    [HideInInspector]
    public Rigidbody agentRb;
    AgentSettings m_AgentSettings;
    BehaviorParameters m_BehaviourParameters;
    public Vector3 initialPosition;
    public float rotationSign;

    EnvironmentParameters m_ResetParameters;

    public override void Initialize()
    {
        // (?)
        AgentEnvController envController = GetComponent<AgentEnvController>();
        if (envController != null)
        {
            m_Existential = 1f / envController.MaxEnvironmentSteps;
        }
        else
        {
            m_Existential = 1f / MaxStep;
        }

        // Setting the team for the Agent
        m_BehaviourParameters = gameObject.GetComponent<BehaviorParameters>();
        if (m_BehaviourParameters.TeamId == (int)Team.Blue)
        {
            team = Team.Blue;
            initialPosition = new Vector3(transform.position.x - 5f, .5f, transform.position.z);
            rotationSign = 1f;
        }
        else
        {
            team = Team.Purple;
            initialPosition = new Vector3(transform.position.x + 5f, .5f, transform.position.z);
            rotationSign = -1f;
        }

        if (position == Position.Generic)
        {
            Debug.Log("Generic Agent");
        }

        m_AgentSettings = FindObjectOfType<AgentSettings>();
        agentRb = GetComponent<Rigidbody>();

        m_ResetParameters = Academy.Instance.EnvironmentParameters;
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Set Rewards
    }

    private void OnCollisionEnter(Collision c)
    {
        if (c.gameObject.CompareTag("ball"))
        {
            // Add a reward

        }
    }

    public override void OnEpisodeBegin()
    {
        m_BallTouched = m_ResetParameters.GetWithDefault("ball_touched", 0);
    }

}

