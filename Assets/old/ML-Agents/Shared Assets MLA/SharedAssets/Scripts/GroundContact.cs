using UnityEngine;
using Unity.MLAgents;

namespace Unity.MLAgentsExamples
{
    /// <summary>
    /// This class contains logic for locomotion agents with joints which might make contact with the ground.
    /// By attaching this as a component to those joints, their contact with the ground can be used as either
    /// an observation for that agent, and/or a means of punishing the agent for making undesirable contact.
    /// </summary>
    [DisallowMultipleComponent]
    public class GroundContact : MonoBehaviour
    {
        [HideInInspector] public Agent agent;

        [Header("Ground Check")] public bool agentDoneOnGroundContact; // Whether to reset agent on ground contact.
        public bool penalizeGroundContact; // Whether to penalize on contact.
        public bool rewardOnContact;
        public bool isFoot;
        public bool isBody;
        public float groundContactPenalty; // Penalty amount (ex: -1).
        public bool touchingObject;
        public bool touchingGround;
        public bool touchingStairs;
        const string k_Ground = "ground"; // Tag of ground object.
        const string k_Stairs = "stairs";
        const string k_Object = "object";

        /// <summary>
        /// Check for collision with ground, and optionally penalize agent.
        /// </summary>
        void OnCollisionEnter(Collision col)
        {
            if (col.transform.CompareTag(k_Ground))
            {
                touchingGround = true;

                if (penalizeGroundContact)
                {
                    agent.SetReward(groundContactPenalty);
                }

                if (rewardOnContact)
                {
                    agent.AddReward(groundContactPenalty);
                }

                if (agentDoneOnGroundContact)
                {
                    agent.EndEpisode();
                    //Debug.Log($"agent done {col.gameObject.name} {this.name}");
                }
            }

            if (col.transform.CompareTag(k_Stairs))
            {
                touchingStairs = true;
                if(isFoot)
                {
                    agent.AddReward(0.002f);
                }
                if (isBody)
                {
                    //Debug.Log($"agent done {col.gameObject.name}");
                    agent.SetReward(-1f);
                    agent.EndEpisode();
                }
            }

            if (col.transform.CompareTag(k_Object))
            {
                touchingObject = true;
                if (isFoot)
                {
                    agent.AddReward(0.002f);
                }
            }
        }

        void OnCollisionExit(Collision other)
        {
            if (other.transform.CompareTag(k_Ground))
            {
                touchingGround = false;
            }

            if (other.transform.CompareTag(k_Stairs))
            {
                touchingStairs = false;
            }

            if (other.transform.CompareTag(k_Object))
            {
                touchingObject = false;
            }
        }
    }
}
