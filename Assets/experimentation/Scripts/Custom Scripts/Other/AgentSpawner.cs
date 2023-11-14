using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;

public class AgentSpawner : MonoBehaviour
{


    public GameObject agent;

    public float SpawnTimer = 0f;

    public GameObject parentEnv;

    private Vector3 agentLocation;
    private Quaternion agentRotation;
    private Vector3 centerPosition;



    // Start is called before the first frame update
    void Start()
    {
        agentLocation = agent.transform.position;
        agentRotation = agent.transform.rotation;

        centerPosition = new Vector3(0f, 5f, 0f);
        StartCoroutine(SpawnCoroutine());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Spawner()
    {
        Instantiate(agent, centerPosition, agentRotation, parentEnv.transform);
    }

    IEnumerator SpawnCoroutine()
    {
        while(true)
        {
            yield return new WaitForSeconds(SpawnTimer);
            Spawner();
        }
        
    }
}
