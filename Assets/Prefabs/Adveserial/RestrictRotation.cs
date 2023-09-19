using UnityEngine;

public class RestrictRotation : MonoBehaviour
{
    private Quaternion originalRotation;
    private Vector3 originalPosition;
    public Transform body;

    void Start()
    {
       
    }

     void FixedUpdate()
    {
        Quaternion restrictedRotation = Quaternion.Euler(0f, body.localEulerAngles.y, 0f);
        transform.position = new Vector3(body.position.x, transform.position.y, body.position.z);
        // Apply the restricted rotation to the child object
        transform.localRotation = restrictedRotation;
    }
}
