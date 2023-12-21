using UnityEngine;

public class CollisionCheckSlip : MonoBehaviour
{
    const string k_Agent = "agent";
    public string managerTag = "manager";  // Tag of the manager object
    private BananaManager manager;

    [SerializeField]
    public bool touchingAgent = false;

    [Header("Physics Material")]
    public PhysicMaterial p_Slippery;

    void Start()
    {
        // Find the manager object using its tag
        GameObject managerObject = GameObject.FindGameObjectWithTag(managerTag);
        if (managerObject != null)
        {
            // Get the manager script from the manager object
            manager = managerObject.GetComponent<BananaManager>();
        }
        else
        {
            Debug.LogError("Manager object not found!");
        }
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.transform.CompareTag(k_Agent))
        {
            touchingAgent = true;

            // Notify the manager to reset the timer
            if (manager != null)
            {
                manager.ResetTimerForChildObject();
            }
        }
    }

    void OnCollisionExit(Collision other)
    {
        if (other.transform.CompareTag(k_Agent))
        {
            touchingAgent = false;
        }
    }
}