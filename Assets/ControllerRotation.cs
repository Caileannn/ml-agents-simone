using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerRotation : MonoBehaviour
{
    public Transform controller;
    public Transform walker;

    private Vector3 zeroPosition;

    float rotationSpeed = 2f;
    // Update is called once per frame
    void Start()
    {
        zeroPosition = controller.position;
    }

    void Update()
    {
        Rotate();
    }

    void Rotate()
    {
        if (Input.GetKey(KeyCode.LeftArrow)) 
        {
            Debug.Log("LArrow");
            Quaternion targetRotation = Quaternion.Euler(0, 1 * 90.0f, 0) * transform.rotation;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            // Do something
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            Debug.Log("RArrow");
            Quaternion targetRotation = Quaternion.Euler(0, -1 * 90.0f, 0) * transform.rotation;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            // Do something
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            Debug.Log("UArrow");
            // Calculate the target position based on user input.
            Vector3 targetPosition = controller.position + (controller.forward * 10);

            // Use Slerp to smoothly interpolate between the current position and the target position.
            controller.position = Vector3.Slerp(controller.position, targetPosition, rotationSpeed * Time.deltaTime);
            // Do something
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            Debug.Log("DArrow");
            // Do something
        }
        else
        {
            Vector3 targetPosition = new Vector3(walker.position.x, walker.position.y + 3f, walker.position.z);
            controller.position = Vector3.Slerp(controller.position, targetPosition, rotationSpeed * Time.deltaTime);
        }
    }
}
