using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;

public class SpawnCube : MonoBehaviour
{
    public GameObject projectile;
    public Transform seat;
    public float rate = 3f;
    float time;
    public float speed = 100f;
    public float mass = 1f;
    public float size = 1f;
    Vector3 initPos = Vector3.zero;
    float sin = 0;

    private void Start()
    {
        time = rate;
        initPos = transform.position;
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        // GameObject agent = GameObject.Find("Seat");

        Vector3 forceDir = seat.transform.position - transform.position;

        time -= Time.fixedDeltaTime;

        if(time <= 0f)
        {
            GameObject cube = Instantiate(projectile, transform.position, transform.rotation);
            cube.GetComponent<Rigidbody>().AddForce(forceDir * speed);
            cube.GetComponent<Rigidbody>().AddTorque(forceDir);
            cube.GetComponent<Rigidbody>().mass = mass;
            cube.transform.localScale = Vector3.one * size;
            time = rate;
            Destroy(cube, time*2f);
        }

        transform.position = new Vector3(initPos.x, initPos.y, initPos.z + ((Mathf.PerlinNoise1D(sin) * 2f)-1f) * 25);
        sin += 0.01f;

        

        
    }
}
