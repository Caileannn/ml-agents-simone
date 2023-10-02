using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockOrientation : MonoBehaviour
{
    // Update is called once per frame
    void FixedUpdate()
    {
        
    }

    public void UpdateRotation(Transform seat) 
    {
        Vector3 newRotation = new Vector3(0f, seat.rotation.eulerAngles.y, 0f);
        transform.rotation = Quaternion.Euler(newRotation);
        Vector3 t_position = new Vector3(seat.position.x, 2.2f, seat.position.z);
        transform.position = t_position;
    }
}