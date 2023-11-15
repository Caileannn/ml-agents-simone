using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Unity.MLAgents;
using UnityEngine.Events;
using System;

namespace Unity.MLAgentsExamples
{
    /// <summary>
    /// Utility class to allow target placement and collision detection with an agent
    /// Add this script to the target you want the agent to touch.
    /// Callbacks will be triggered any time the target is touched with a collider tagged as 'tagToDetect'
    /// </summary>
    public class TargetControllerWalker : MonoBehaviour
    {


        [Header("Use Controller")]
        public bool useController = false;

        [Header("Mesh Settings")]
        [SerializeField]
        private MeshFilter modelToChange;

        [SerializeField]
        private Mesh modelToUse;

        [SerializeField]
        [Header("Controller Colour")]
        Material yellow;

        [Header("Collider Tag To Detect")]
        public string tagToDetect = "agent"; //collider tag to detect 

        [Header("Target Placement")]
        public float spawnRadius; //The radius in which a target can be randomly spawned.
        public bool respawnIfTouched; //Should the target respawn to a different position when touched

        [Header("Target Fell Protection")]
        public bool respawnIfFallsOffPlatform = true; //If the target falls off the platform, reset the position.
        public float fallDistance = 5; //distance below the starting height that will trigger a respawn 

        public Agent chair;
        private Vector3 m_startingPos; //the starting position of the target

        [System.Serializable]
        public class TriggerEvent : UnityEvent<Collider>
        {
        }

        [Header("Trigger Callbacks")]
        public TriggerEvent onTriggerEnterEvent = new TriggerEvent();
        public TriggerEvent onTriggerStayEvent = new TriggerEvent();
        public TriggerEvent onTriggerExitEvent = new TriggerEvent();

        [System.Serializable]
        public class CollisionEvent : UnityEvent<Collision>
        {
        }

        [Header("Collision Callbacks")]
        public CollisionEvent onCollisionEnterEvent = new CollisionEvent();
        public CollisionEvent onCollisionStayEvent = new CollisionEvent();
        public CollisionEvent onCollisionExitEvent = new CollisionEvent();

        // Set Env Controller
        public GameObject area;
        [HideInInspector]
        public EnvironmentController envController;

        // Start is called before the first frame update

        void OnEnable()
        {
            envController = area.GetComponent<EnvironmentController>();

            if (useController)
            {
                // Disable Graviy, Collider, RespwawnIfTouched, RespawnIfFallOfPlatform
                respawnIfFallsOffPlatform = false;
                respawnIfTouched = false;
                this.GetComponent<Collider>().enabled = false;
                this.GetComponent<Rigidbody>().useGravity = false;
                this.GetComponent<ControllerRotation>().enabled = true;
                
                // Set the scale of the controller to be slightly smaller
                this.transform.localScale = Vector3.one * 3f;

                // Set the material & mesh
                modelToChange.mesh = modelToUse;
                this.GetComponent<Renderer>().material = yellow;
            }
            else
            {
                this.GetComponent<ControllerRotation>().enabled = false;
            }

            m_startingPos = transform.position;

            if (respawnIfTouched)
            {
                MoveTargetToRandomPosition();
            }
        }

        void Update()
        {
            if (respawnIfFallsOffPlatform)
            {
                if (transform.position.y < m_startingPos.y - fallDistance)
                {
                    Debug.Log($"{transform.name} Fell Off Platform");
                    MoveTargetToRandomPosition();
                }
            }
        }

        /// <summary>
        /// Moves target to a random position within specified radius.
        /// </summary>
        public void MoveTargetToRandomPosition()
        {
            var newTargetPos = m_startingPos + (Random.insideUnitSphere * spawnRadius);
            newTargetPos.y = m_startingPos.y;
            transform.position = newTargetPos;
        }

        private void OnCollisionEnter(Collision col)
        {
            if (col.transform.CompareTag(tagToDetect))
            {
                onCollisionEnterEvent.Invoke(col);
                if (respawnIfTouched)
                {
                    chair.SetReward(1f);
                    MoveTargetToRandomPosition();
                }
            }
        }

        private void OnCollisionStay(Collision col)
        {
            if (col.transform.CompareTag(tagToDetect))
            {
                onCollisionStayEvent.Invoke(col);
            }
        }

        private void OnCollisionExit(Collision col)
        {
            if (col.transform.CompareTag(tagToDetect))
            {
                onCollisionExitEvent.Invoke(col);
            }
        }

        private void OnTriggerEnter(Collider col)
        {
            if (col.CompareTag(tagToDetect))
            {
                onTriggerEnterEvent.Invoke(col);
            }
        }

        private void OnTriggerStay(Collider col)
        {
            if (col.CompareTag(tagToDetect))
            {
                onTriggerStayEvent.Invoke(col);
            }
        }

        private void OnTriggerExit(Collider col)
        {
            if (col.CompareTag(tagToDetect))
            {
                onTriggerExitEvent.Invoke(col);
            }
        }
    }
}

