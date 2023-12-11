using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Unity.MLAgents;
using Unity.MLAgentsExamples;
using UnityEngine.Events;
using System;
using Unity.VisualScripting;

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

        [Header("End Episode After Touch")]
        public bool m_EndEpisodeOnTouch = false;

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

        [HideInInspector]
        public int m_SwitchStairs = 1;
        public bool m_InitialSpawn = true;
        public Vector3 m_StartingStairPosition = Vector3.zero;
        public List<GameObject> m_StairChildren = new List<GameObject>();

        [Header("Stair Collection")]
        public GameObject m_StairCollection;

       


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

            if (chair.gameObject.GetComponent<Walker>().m_ScramblerTraining)
            {
                foreach (Transform child in m_StairCollection.transform)
                {
                    m_StairChildren.Add(child.gameObject);
                }
            }

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
            // If scrambler training is on, spawn target on top of stairs
            try
            {
                if (chair.gameObject.GetComponent<Walker>().m_ScramblerTraining)
                {

                    if (m_InitialSpawn)
                    {
                        m_InitialSpawn = false;
                        m_SwitchStairs = 1;

                        m_StartingStairPosition = m_StairChildren[m_SwitchStairs - 1].transform.position;
                        SetScramlberPosition(m_SwitchStairs);


                    }

                    else

                    {

                        if (m_SwitchStairs == 1)
                        {
                            m_SwitchStairs++;
                        }

                        else if (m_SwitchStairs == 2)
                        {
                            m_SwitchStairs--;
                        }

                        SetScramlberPosition(m_SwitchStairs);
                    }

                }

                else

                {
                    var newTargetPos = m_startingPos + (Random.insideUnitSphere * spawnRadius);
                    newTargetPos.y = m_startingPos.y;
                    transform.position = newTargetPos;
                }
            }
            catch
            {
                var newTargetPos = m_startingPos + (Random.insideUnitSphere * spawnRadius);
                newTargetPos.y = m_startingPos.y;
                transform.position = newTargetPos;
            }

        }

        public void SetScramlberPosition(int stairSwitch)
        {
            SetRandomPositionForStairs(m_StairChildren[stairSwitch-1]);

            var sHeight = (m_StairChildren[stairSwitch-1].GetComponent<StairsScaler>().m_ScaleY*3) + 18;

            var center = m_StairChildren[stairSwitch-1].GetComponentInChildren<Renderer>().bounds.center;

            transform.position = new Vector3(center.x, sHeight, center.z);
           
        }

        public void SetRandomPositionForStairs(GameObject stair)
        {
            // Set random position of a stair
            // Check if anything is already there
            // Change its position and place target there
            var newPos = m_StartingStairPosition + (Random.insideUnitSphere * spawnRadius);
            // Set new stair height
            stair.GetComponent<StairsScaler>().m_ScaleY = Academy.Instance.EnvironmentParameters.GetWithDefault("stair_height", 0.01f);
            newPos.y = m_StartingStairPosition.y;
            Collider[] hitColliders;
            bool freeSpace = true;
            bool breakLoop = false;

            while (!breakLoop) 
            {
                hitColliders = Physics.OverlapBox(newPos, (stair.transform.localScale * 40) / 2, Quaternion.identity);
                if (hitColliders.Length > 0)
                {
                    freeSpace = true;
                    foreach (Collider collider in hitColliders)
                    {
                        if (collider.tag == "agent")
                        {
                            newPos = m_StartingStairPosition + (Random.insideUnitSphere * spawnRadius);
                            newPos.y = m_StartingStairPosition.y;
                            freeSpace = false;
                        }
                    }

                    if (freeSpace)
                    {
                        breakLoop = true;
                    }
                }
            }

            stair.transform.position = newPos;
        }

        

        private void OnCollisionEnter(Collision col)
        {
            if (col.transform.CompareTag(tagToDetect))
            {
                onCollisionEnterEvent.Invoke(col);
                if (respawnIfTouched)
                {
                    chair.AddReward(1f);
                    if (m_EndEpisodeOnTouch)
                    {
                        chair.EndEpisode();
                    }
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

