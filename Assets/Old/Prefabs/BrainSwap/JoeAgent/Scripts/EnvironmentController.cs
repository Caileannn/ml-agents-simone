using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using Unity.MLAgents;
using UnityEngine;

public class EnvironmentController : MonoBehaviour
{

	public SwapAgent agent;

    [HideInInspector] public Boolean targetChoice;

    // Start is called before the first frame update
    void Start()
    {
        targetChoice = agent.targetChoice;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void targetTouched(GameObject collide) {
        // Add a Reward

        if (targetChoice && collide.name == "Green_Target")
        {
            GlobalVars.g_Success += 1;
        }
        else if (!targetChoice && collide.name == "Red_Target")
        {
            GlobalVars.g_Success += 1;
        }
        else
        {
            GlobalVars.g_Failure += 1;
            agent.AddReward(-1f);
            agent.EndEpisode();
        }
		
	}
}
