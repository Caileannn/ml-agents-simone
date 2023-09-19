using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpawnObstacles : MonoBehaviour
{

    /*
     (1) Spawn Object @ the start of each episode / Delete Objetcs in current episode

     (2) Know the position of the agent & target

     (3) Check if we can spanw the Object in a new random position within the radius of the floor dims
     
     */


    // Position of Agent & Target (updated everytime the script is run)

    Transform agentPosition;

    Transform targetPosition;

    // List of Obstacles
    public GameObject[] obstacles;

    // Vetcor which will hold all the current objects which have been instantiated
    private List<GameObject> obstaclesList = new List<GameObject>();

    // Spawn Radius
    float spawnRadius = 25.0f;

    // Overlap Bounds
    float overlapBounds = 2.0f;

    // Overlap Check    
    bool overlapCheck = false;

    // Number of Obstacles to Spawn
    public uint obstCount = 0;

    public Vector3 envPosition;

    public void Spawn()
    {

        // Set the parent
        Transform parent = GetComponent<Transform>();
        
        agentPosition = transform.Find("Chair");
        targetPosition = transform.Find("Target_Cube");

        // Initially Spawn the Objects
        // If obstacles don't already exist -> create a list of obstacles
        if (!obstaclesList.Any())
        {
            for (int t = 0; t < obstCount; t++)
            {
                int randomIndex = Random.Range(0, obstacles.Length);
                GameObject obsPrefab = obstacles[randomIndex];

                // Spawn Object & Add to list
                GameObject spawnedObject = Instantiate(obsPrefab, SetPosition(), Quaternion.identity);
               
                
                obstaclesList.Add(spawnedObject);
            }
            
        }
        else
        {
            // Update the position of the Objects
            foreach (GameObject obstacle in obstaclesList) 
            {
                obstacle.transform.position = SetPosition();
            }
        }
    }

    public void Spawn(Vector3 targetLocation)
    {

        agentPosition = transform.Find("Chair");
        targetPosition = transform.Find("Target_Cube");

        
        // Initially Spawn the Objects
        // If obstacles don't already exist -> create a list of obstacles
        if (!obstaclesList.Any())
        {
            for (int t = 0; t < obstCount; t++)
            {
                int randomIndex = Random.Range(0, obstacles.Length);
                GameObject obsPrefab = obstacles[randomIndex];

                // Spawn Object & Add to list
                GameObject spawnedObject = Instantiate(obsPrefab, SetPosition(), Quaternion.identity);
                obstaclesList.Add(spawnedObject);
            }

        }
        else
        {
            // Update the position of the Objects
            foreach (GameObject obstacle in obstaclesList)
            {
                obstacle.transform.position = SetPosition(targetLocation);
            }
        }
    }

    Vector3 SetPosition()
    {
        Vector3 correctPosition = Vector3.zero;
        envPosition = new Vector3(transform.Find("Ground").GetComponent<Renderer>().bounds.center.x, 0f, transform.Find("Ground").GetComponent<Renderer>().bounds.center.z);
        overlapCheck = false;

        while(!overlapCheck)
        {
            // Generate a random position within a defined r
            Vector2 randomCirclePosition = Random.insideUnitCircle * spawnRadius;
            Vector3 randomSpawnPosition = new Vector3(envPosition.x + randomCirclePosition.x, 0.5f, envPosition.z + randomCirclePosition.y);

            // Check dist between agent, target, and obstacle

            float distanceX = Mathf.Abs(randomSpawnPosition.x - targetPosition.position.x);
            float distanceZ = Mathf.Abs(randomSpawnPosition.z - targetPosition.position.z);

            

            if (distanceX > overlapBounds && distanceZ > overlapBounds)
            {
                // distance between agent and obstacle
                distanceX = Mathf.Abs(randomSpawnPosition.x - agentPosition.position.x);
                distanceZ = Mathf.Abs(randomSpawnPosition.z - agentPosition.position.z);

                if (distanceX > overlapBounds && distanceZ > overlapBounds)
                {
                    overlapCheck = true;
                    correctPosition = randomSpawnPosition;
                }
            }
        }

        return correctPosition;
    }

    Vector3 SetPosition(Vector3 targetLocation)
    {
        Vector3 correctPosition = Vector3.zero;
        envPosition = new Vector3(transform.Find("Ground").GetComponent<Renderer>().bounds.center.x, 0f, transform.Find("Ground").GetComponent<Renderer>().bounds.center.z);
        overlapCheck = false;

        while (!overlapCheck)
        {
            // Generate a random position within a defined r
            Vector2 randomCirclePosition = Random.insideUnitCircle * spawnRadius;
            Vector3 randomSpawnPosition = new Vector3(envPosition.x + randomCirclePosition.x, 0.5f, envPosition.z + randomCirclePosition.y);

            // Check dist between agent, target, and obstacle
            float distanceX = Mathf.Abs(randomSpawnPosition.x - targetLocation.x);
            float distanceZ = Mathf.Abs(randomSpawnPosition.z - targetLocation.z);



            if (distanceX > overlapBounds && distanceZ > overlapBounds)
            {
                // distance between agent and obstacle
                distanceX = Mathf.Abs(randomSpawnPosition.x - agentPosition.position.x);
                distanceZ = Mathf.Abs(randomSpawnPosition.z - agentPosition.position.z);

                if (distanceX > overlapBounds && distanceZ > overlapBounds)
                {
                    overlapCheck = true;
                    correctPosition = randomSpawnPosition;
                }
            }
        }

        return correctPosition;
    }
}
