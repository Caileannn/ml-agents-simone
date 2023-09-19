using UnityEngine;

public class BallController : MonoBehaviour
{

	public GameObject area;
	[HideInInspector]
	public AgentEnvController envController;
	public string purpleBallTag; // Will check if collided w/ purple agent
	public string blueBallTag; // Will chek if collided w/ blue agent
    
    void Start()
    {
        envController = area.GetComponent<AgentEnvController>();
    }

    // Update is called once per frame
    void OnCollisionEnter(Collision col)
    {
		// Ball touched by purple agent
        if (col.gameObject.CompareTag(purpleBallTag))
		{
			//envController.BallTouched(Team.Purple);
		} 

		// Ball touched by blue agent
		if (col.gameObject.CompareTag(blueBallTag))
		{
            envController.BallTouched(Team.Blue);
		} 
    }
}
