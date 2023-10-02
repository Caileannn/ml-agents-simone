using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Unity.MLAgents;
using UnityEngine.Events;
using Unity.VisualScripting;
using System.Diagnostics.Tracing;

namespace Unity.MLAgentsExamples
{
    /// <summary>
    /// Utility class to allow target placement and collision detection with an agent
    /// Add this script to the target you want the agent to touch.
    /// Callbacks will be triggered any time the target is touched with a collider tagged as 'tagToDetect'
    /// </summary>
    public class TargetControllerStairs : MonoBehaviour
    {



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

        // List of Stair prefabs
        public GameObject[] m_Stairs;
        // Current Stair Level
        private int m_StairsLevel = 1;
        // List of Active Stairs (max:2)
        private GameObject[] m_ActiveStairs = new GameObject[2];
        // The last stair which had the target on it
        private int m_LastStairs = 0;
        // Stairs Parent
        public GameObject m_StairsParent;

        private bool m_Hitted = false;

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

            m_startingPos = m_StairsParent.transform.position;
            if (respawnIfTouched)
            {
                MoveStairsInit();
            }
        }

        void Update()
        {
            if (respawnIfFallsOffPlatform)
            {
                if (transform.position.y < m_startingPos.y - fallDistance)
                {
                    Debug.Log($"{transform.name} Fell Off Platform");
                    //MoveStairsOnTouch();
                }
            }
        }

        public void MoveStairsInit()
        {
            m_LastStairs = 0;

            // Delete any exisiting objects
            if (m_ActiveStairs[0] != null)
            {
                foreach (var item in m_ActiveStairs)
                {
                    Destroy(item);
                }
            }

            // Check level, Add to Active Array, and Inst prefab
            bool check_stairs;
            Vector3 newStairsPosition = Vector3.zero;
            float yOffset = 0;

            for (int i = 0; i < m_ActiveStairs.Length; i++)
            {
                check_stairs = false;
                while (!check_stairs)
                {
                    Bounds bounds = m_Stairs[m_StairsLevel].GetComponent<Renderer>().bounds;
                    foreach (Transform child in m_Stairs[m_StairsLevel].transform)
                    {
                        bounds.Encapsulate(child.gameObject.GetComponent<Renderer>().bounds);
                    }

                    float boundsHeight = bounds.size.y;
                    
                    if(m_StairsLevel == 0)
                    {
                        yOffset = 0.1f;
                    }
                    else if (m_StairsLevel == 1)
                    {
                        yOffset = -0.3f;
                    }
                    else if(m_StairsLevel == 2)
                    {
                        yOffset = -0.5f;
                    }

                    newStairsPosition = m_startingPos + (Random.insideUnitSphere * spawnRadius);
                    newStairsPosition.y = m_startingPos.y + (boundsHeight/2f + yOffset);
                    
                    if (!Physics.CheckBox(newStairsPosition, m_Stairs[m_StairsLevel].transform.localScale / 2f, Quaternion.identity))
                    {
                        check_stairs = true;
                    }

                    //check_stairs = true;
                
                }

                m_ActiveStairs[i] = Instantiate(m_Stairs[m_StairsLevel], newStairsPosition, Quaternion.identity, m_StairsParent.transform);
            }

            // Pick a random Stair, and move object to the top: we also have to get the height
            var randomInt = Random.Range(0, 1);
            var randomStair = m_ActiveStairs[randomInt];
            Vector3 newTargetPos = randomStair.transform.position;
            newTargetPos.y = newTargetPos.y + (randomStair.transform.localScale.y + 2f);
            transform.position = newTargetPos;
            m_LastStairs = randomInt;
        }

        public void MoveStairsOnTouch()
        {
            // Check which stairs the target was on. 
            bool check_stairs;
            float yOffset = 0f;
            Vector3 newStairsPosition = Vector3.zero;

            check_stairs = false;

            while (!check_stairs)
            {
                Bounds bounds = m_Stairs[m_StairsLevel].GetComponent<Renderer>().bounds;
                foreach (Transform child in m_Stairs[m_StairsLevel].transform)
                {
                    bounds.Encapsulate(child.gameObject.GetComponent<Renderer>().bounds);
                }

                float boundsHeight = bounds.size.y;

                if (m_StairsLevel == 0)
                {
                    yOffset = 0.1f;
                }
                else if (m_StairsLevel == 1)
                {
                    yOffset = -0.3f;
                }
                else if (m_StairsLevel == 2)
                {
                    yOffset = -0.5f;
                }

                newStairsPosition = m_startingPos + (Random.insideUnitSphere * spawnRadius);
                newStairsPosition.y = m_startingPos.y + (boundsHeight / 2f + yOffset);

                if (!Physics.CheckBox(newStairsPosition, m_Stairs[m_StairsLevel].transform.localScale / 2f, Quaternion.identity))
                {
                    check_stairs = true;
                }
            }

            if (m_LastStairs == 1) { m_LastStairs--; } else { m_LastStairs++; }

            m_ActiveStairs[m_LastStairs].transform.position = newStairsPosition;

            GameObject randomStair = m_ActiveStairs[m_LastStairs];
            Vector3 newTargetPos = randomStair.transform.position;
            newTargetPos.y = newTargetPos.y + (randomStair.transform.localScale.y + 2f);
            transform.position = newTargetPos;
        }
        /// <summary>
        /// Moves target to a random position within specified radius.
        /// </summary>
        public void MoveTargetToRandomPosition()
        {

            // (1) Get the current Stairs Level using the Academy Singleton
            // (2) Spawn stair while checking the location for collisions - there will be 4 positions
            // (3) Spawn cube on top of stairs, Scaling with height/level

            // Randomsise Spawn Location

            bool check_stairs = false;
            Vector3 newStairPosition = Vector3.zero;

            while(!check_stairs)
            {
                newStairPosition = m_startingPos + (Random.insideUnitSphere * spawnRadius);
                newStairPosition.y = m_startingPos.y;
                if (!Physics.CheckBox(newStairPosition, transform.localScale / 2f, Quaternion.identity))
                {
                    check_stairs = true;
                }
            }

            // Set new position

            bool check_position = false;
            Vector3 newTargetPos = Vector3.zero;

            while(!check_position)
            {
                newTargetPos = m_startingPos + (Random.insideUnitSphere * spawnRadius);
                newTargetPos.y = m_startingPos.y;
                if (!Physics.CheckBox(newTargetPos, transform.localScale / 2f, Quaternion.identity))
                {
                    check_position = true;
                }
            }
            
            transform.position = newTargetPos;
        }

        private void OnCollisionEnter(Collision col)
        {
            if (col.transform.CompareTag(tagToDetect) && (m_Hitted == false))
            {
                m_Hitted = true;
                onCollisionEnterEvent.Invoke(col);
                if (respawnIfTouched)
                {   
                    chair.AddReward(1f);
                    MoveStairsOnTouch();
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
            if (col.transform.CompareTag(tagToDetect) && (m_Hitted == true))
            {
                m_Hitted = false;
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

