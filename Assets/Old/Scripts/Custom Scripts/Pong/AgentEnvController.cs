using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;

public class AgentEnvController : MonoBehaviour
{
    [System.Serializable]

	public class PlayerInfo
	{
		public AgentClass Agent;
		[HideInInspector]
		public Vector3 StartingPosition;
		[HideInInspector]
		public Quaternion StratingRotation;
	}

	/// <summary>
    /// Max Academy steps before this platform resets
    /// </summary>
    /// <returns></returns>
    [Tooltip("Max Environment Steps")] public int MaxEnvironmentSteps = 25000;
	
	public GameObject ball;
	[HideInInspector]
    public Rigidbody ballRb;
    Vector3 m_BallStartingPos;

	public List<PlayerInfo> AgentsList = new List<PlayerInfo>();

	private AgentSettings m_AgentSettings;

	//private SimpleMultiAgentGroup m_BlueAgentGroup;
	//private SimpleMultiAgentGroup m_PurpleAgentGroup;

	private int m_ResetTimer;

	void Start()
	{
		m_AgentSettings = FindObjectOfType<AgentSettings>();

		// Initialise Team Manager
		//m_BlueAgentGroup = new SimpleMultiAgentGroup();
		//m_PurpleAgentGroup = new SimpleMultiAgentGroup();
		ballRb = ball.GetComponent<Rigidbody>();
        
		m_BallStartingPos = new Vector3(ball.transform.position.x, ball.transform.position.y, ball.transform.position.z);

		foreach (var item in AgentsList)
		{
			item.StartingPosition = item.Agent.transform.position;
			item.StratingRotation = item.Agent.transform.rotation;

			if (item.Agent.team == Team.Blue)
			{
				//m_BlueAgentGroup.RegisterAgent(item.Agent);
			}
			else
			{
				//m_PurpleAgentGroup.RegisterAgent(item.Agent);
			}
		}
		ResetScene();
	}

	void FixedUpdate()
	{
		m_ResetTimer += 1;
        if (m_ResetTimer >= MaxEnvironmentSteps && MaxEnvironmentSteps > 0)
        {
            // m_BlueAgentGroup.GroupEpisodeInterrupted();
            // m_PurpleAgentGroup.GroupEpisodeInterrupted();
            foreach (var item in AgentsList)
            {
                item.Agent.EpisodeInterrupted();
            }
            ResetScene();
        }
	}

	public void ResetBall()
    {
        var randomPosX = Random.Range(-2.5f, 2.5f);
        var randomPosZ = Random.Range(-2.5f, 2.5f);

        ball.transform.position = m_BallStartingPos + new Vector3(0f, 0f, randomPosZ);
        ballRb.velocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;

    }

	public void BallTouched(Team scoredTeam)
    {
        if (scoredTeam == Team.Blue)
        {
            // Attacker Reward

            foreach (var item in AgentsList)
            {
                if (item.Agent.team == Team.Blue)
                {
                    item.Agent.AddReward(1 - (float)m_ResetTimer / MaxEnvironmentSteps);
                }
                else
                {
                    item.Agent.AddReward(-1);
                }
            }
            //m_BlueAgentGroup.AddGroupReward(1 - (float)m_ResetTimer / MaxEnvironmentSteps);
            //m_PurpleAgentGroup.AddGroupReward(-1);

            foreach (var item in AgentsList)
            {
                item.Agent.EndEpisode();
            }
        }

        ResetScene();
    }

	public void ResetScene()
    {
        m_ResetTimer = 0;

        //Reset Agents
        foreach (var item in AgentsList)
        {
            var randomPosX = Random.Range(-5f, 5f);
            var newStartPos = item.Agent.initialPosition + new Vector3(0f, 0f, randomPosX);
            var rot = item.Agent.rotationSign * Random.Range(80.0f, 100.0f);
            var newRot = Quaternion.Euler(0, rot, 0);
        }

        //Reset Ball
        ResetBall();
    }
}
