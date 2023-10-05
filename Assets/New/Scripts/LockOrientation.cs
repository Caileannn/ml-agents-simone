using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockOrientation : MonoBehaviour
{
    public float y_offset = 0.0f;
    public float z_offset = 0.0f;

    // Update is called once per frame
    void FixedUpdate()
    {
        
    }

    public void UpdateRotation(Transform seat) 
    {
        Vector3 newRotation = new Vector3(0f, seat.rotation.eulerAngles.y, 0f);
        transform.rotation = Quaternion.Euler(newRotation);
        Vector3 t_position = new Vector3(seat.position.x, y_offset, seat.position.z + z_offset);
        transform.position = t_position;
    }
}