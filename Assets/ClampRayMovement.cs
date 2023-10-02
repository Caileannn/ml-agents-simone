using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClampRayMovement : MonoBehaviour
{
    // Start is called before the first frame update

    private Transform m_InitalTransform;

    void Start()
    {
        m_InitalTransform = transform;
    }

    // Update is called once per frame
    void Update()
    {
        // Y axis stay the same, Rotation Y update, and no other;

        Vector3 currentPosition = transform.position;
        Vector3 newPosition = new Vector3(currentPosition.x, m_InitalTransform.position.y, currentPosition.z);

        float currentRotationY = transform.eulerAngles.y;
        Vector3 newRotation = new Vector3(0f, currentRotationY, 0f);
        transform.rotation = Quaternion.Euler(newRotation);
    }
}
