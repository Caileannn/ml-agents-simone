using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BananaManager : MonoBehaviour
{
    public float ResetTimer = 5.0f;
    public float timer;
    public PhysicMaterial ground;

    private void OnEnable()
    {
        timer = 0f;
        ground.staticFriction = 1000f;
        ground.dynamicFriction = 1000f;
    }

    private void OnDisable()
    {
        ground.staticFriction = 1000f;
        ground.dynamicFriction = 1000f;
    }


    public void ResetTimerForChildObject()
    {
        timer = ResetTimer;
        Debug.Log("Timer reset by child object");
    }

    void FixedUpdate()
    {
        // Add your logic based on the timer value here
        if (timer > 0)
        {
            timer -= Time.fixedDeltaTime;
            ground.staticFriction = 0f;
            ground.dynamicFriction = 0f;
        }

        if (timer <= 0)
        {
            // Timer has reached zero, perform your timer-expired action here
            Debug.Log("Timer expired!");
            ground.staticFriction = 1000f;
            ground.dynamicFriction = 1000f;
        }
    }
}
