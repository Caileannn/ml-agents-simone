using System;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgentsExamples;
using Unity.MLAgents.Sensors;
using BodyPart = Unity.MLAgentsExamples.BodyPart;
using Random = UnityEngine.Random;

public class SwapAgent : Agent
{
    
    //The direction an agent will walk during training.
    private Vector3 m_WorldDirToWalk = Vector3.right;

    [HideInInspector] public Boolean m_RandomActuations = false;

    [HideInInspector] public float m_ActuationsValue = 0f;

    [HideInInspector] public Transform redTarget; //Target the agent will walk towards during training.
	
	[HideInInspector] public Transform greenTarget; //Target the agent will walk towards during training.


    [Header("Body Parts")] public Transform body;
    public Transform arml;
    public Transform armr;

    [HideInInspector]
    public SwapModel modelSwapper;

    [HideInInspector]
    public GameObject area;

    Transform target;

	[Header("Boolean Target")] public Boolean targetChoice;

    //This will be used as a stabilized model space reference point for observations
    //Because ragdolls can move erratically during training, using a stabilized reference transform improves learning
    OrientationCubeController m_OrientationCube;

    //The indicator graphic gameobject that points towards the target
    DirectionIndicator m_DirectionIndicator;
    JointDriveController m_JdController;

    //Get the inital Location of Agent at the start of each episode
    Vector3 m_initalAgentPosition;

    [Header("Walk Speed")]
    [Range(0.1f, m_maxWalkingSpeed)]
    [SerializeField]
    [Tooltip(
        "The speed the agent will try to match.\n\n" +
        "TRAINING:\n" +
        "For VariableSpeed envs, this value will randomize at the start of each training episode.\n" +
        "Otherwise the agent will try to match the speed set here.\n\n" +
        "INFERENCE:\n" +
        "During inference, VariableSpeed agents will modify their behavior based on this value " +
        "whereas the CrawlerDynamic & CrawlerStatic agents will run at the speed specified during training "
    )]
    //The walking speed to try and achieve
    private float m_TargetWalkingSpeed = m_maxWalkingSpeed;

    const float m_maxWalkingSpeed = 15; //The max walking speed

    //The current target walking speed. Clamped because a value of zero will cause NaNs
    public float TargetWalkingSpeed
    {
        get { return m_TargetWalkingSpeed; }
        set { m_TargetWalkingSpeed = Mathf.Clamp(value, .1f, m_maxWalkingSpeed); }
    }



    public override void Initialize()
    {
        m_OrientationCube = GetComponentInChildren<OrientationCubeController>();
        // Set targets green/red

        redTarget = GameObject.Find("Red_Target").transform;
        greenTarget = GameObject.Find("Green_Target").transform;
        area = GameObject.Find("Environment");

        //Setup each body part
        m_JdController = GetComponent<JointDriveController>();
        m_JdController.SetupBodyPart(body);
        m_JdController.SetupBodyPart(arml);
        m_JdController.SetupBodyPart(armr);

        foreach (var bp in m_JdController.bodyPartsList)
        {
            Debug.Log("INT: " + bp.rb.transform);
        }

        

		if (targetChoice) { target = greenTarget; }
		else { target = redTarget; }

       
    }

    /// <summary>
    /// Loop over body parts and reset them to initial conditions.
    /// </summary>
    public override void OnEpisodeBegin()
    {
        modelSwapper = area.GetComponent<SwapModel>();

        // Set Init model
        modelSwapper.SwitchModel(3, this);
        // Get initial position of Agent
        m_initalAgentPosition = body.position;

        //Reset all of the body parts
        foreach (var bodyPart in m_JdController.bodyPartsDict.Values)
        {
            bodyPart.Reset(bodyPart);
        }

        //Random start rotation to help generalize
        body.rotation = Quaternion.Euler(0, Random.Range(0.0f, 360.0f), 0);

        UpdateOrientationObjects();

        //Set our goal walking speed
        TargetWalkingSpeed = Random.Range(0.1f, m_maxWalkingSpeed);
    }

    /// <summary>
    /// Add relevant information on each body part to observations.
    /// </summary>
    public void CollectObservationBodyPart(BodyPart bp, VectorSensor sensor)
    {
		//GROUND CHECK
        sensor.AddObservation(bp.groundContact.touchingGround); // Is this bp touching the ground

        //Get velocities in the context of our orientation cube's space
        //Note: You can get these velocities in world space as well but it may not train as well.
        sensor.AddObservation(m_OrientationCube.transform.InverseTransformDirection(bp.rb.velocity));
        sensor.AddObservation(m_OrientationCube.transform.InverseTransformDirection(bp.rb.angularVelocity));

        //Get position relative to hips in the context of our orientation cube's space
        sensor.AddObservation(m_OrientationCube.transform.InverseTransformDirection(bp.rb.position - body.position));

        if (bp.rb.transform != body)
        {
            sensor.AddObservation(bp.rb.transform.localRotation);
            sensor.AddObservation(bp.currentStrength / m_JdController.maxJointForceLimit);
        }
    }

    /// <summary>
    /// Loop over body parts to add them to observation.
    /// </summary>
    public override void CollectObservations(VectorSensor sensor)
    {
        var bodyCount = 0;
        var cubeForward = m_OrientationCube.transform.forward;

        //velocity we want to match
        var velGoal = cubeForward * TargetWalkingSpeed;

        //ragdoll's avg vel
        var avgVel = GetAvgVelocity();

        //current ragdoll velocity. normalized
        sensor.AddObservation(Vector3.Distance(velGoal, avgVel));
        //avg body vel relative to cube
        sensor.AddObservation(m_OrientationCube.transform.InverseTransformDirection(avgVel));
        //vel goal relative to cube
        sensor.AddObservation(m_OrientationCube.transform.InverseTransformDirection(velGoal));

        sensor.AddObservation(m_OrientationCube.transform.InverseTransformPoint(redTarget.transform.position));

        sensor.AddObservation(m_OrientationCube.transform.InverseTransformPoint(greenTarget.transform.position));

        sensor.AddObservation(Quaternion.FromToRotation(body.forward, cubeForward));

        foreach (var bodyPart in m_JdController.bodyPartsList)
        {
            bodyCount++;
            CollectObservationBodyPart(bodyPart, sensor);
        }

        if(bodyCount == 6)
        {
            foreach (var bp in m_JdController.bodyPartsList)
            {
                Debug.Log(bp.rb.transform);
            }
        }
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)

    {
        var bpDict = m_JdController.bodyPartsDict;
        var i = -1;

        var continuousActions = actionBuffers.ContinuousActions;
        bpDict[arml].SetJointTargetRotation(continuousActions[++i] + RandomActuations(), continuousActions[++i] + RandomActuations(), continuousActions[++i] + RandomActuations());
        bpDict[armr].SetJointTargetRotation(continuousActions[++i] + RandomActuations(), continuousActions[++i] + RandomActuations(), continuousActions[++i] + RandomActuations());

        //update joint strength settings
        bpDict[arml].SetJointStrength(continuousActions[++i]);
        bpDict[armr].SetJointStrength(continuousActions[++i]);
    }

    public float RandomActuations()
    {
        if (m_RandomActuations)
        {
            float randomFloat = Random.Range(-m_ActuationsValue, m_ActuationsValue);
            return randomFloat;
        }

        return 0f;
    }

	public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = -Input.GetAxis("Horizontal");
        continuousActionsOut[3] = Input.GetAxis("Vertical");
    }

    //Update OrientationCube and DirectionIndicator
    void UpdateOrientationObjects()
    {
        m_WorldDirToWalk = target.position - body.position;
        m_OrientationCube.UpdateOrientation(body, target);
        if (m_DirectionIndicator)
        {
            m_DirectionIndicator.MatchOrientation(m_OrientationCube.transform);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Debug.Log("L");
            target = redTarget;
            modelSwapper.SwitchModel(0, this);
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            Debug.Log("R");
            target = redTarget;
            modelSwapper.SwitchModel(1, this);
        }

        if (Input.GetKeyDown(KeyCode.UpArrow)) {
            Debug.Log("Noise+");
            m_ActuationsValue += 0.5f;
        }

        if (Input.GetKeyDown(KeyCode.DownArrow)) {
            Debug.Log("Noise-");
            m_ActuationsValue -= 0.5f;
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            if(!m_RandomActuations) 
            {
                Debug.Log("Random Actuations On");
                m_RandomActuations = true;
            } 
            else
            {
                Debug.Log("Random Actuations Off");
                m_ActuationsValue = 0f;
                m_RandomActuations = false;
            }
            
        }


        //Monitor.Log("Random Actuations", m_RandomActuations.ToString(), body, Camera.main);
        //Monitor.Log("Actuation Value", m_ActuationsValue/10, body, Camera.main);
        //Monitor.Log("Actuation Value", m_ActuationsValue, body, Camera.main);
    }

    void FixedUpdate()
    {


        UpdateOrientationObjects();

		var cubeForward = m_OrientationCube.transform.forward;

        // Set reward for this step according to mixture of the following elements.
        // a. Match target speed
        //This reward will approach 1 if it matches perfectly and approach zero as it deviates
        var matchSpeedReward = GetMatchingVelocityReward(cubeForward * TargetWalkingSpeed, GetAvgVelocity());

        // b. Rotation alignment with target direction.
        //This reward will approach 1 if it faces the target direction perfectly and approach zero as it deviates
        var lookAtTargetReward = (Vector3.Dot(cubeForward, body.forward) + 1) * .5F;

        AddReward(matchSpeedReward * lookAtTargetReward);
    }

    Vector3 GetAvgVelocity()
    {
        Vector3 velSum = Vector3.zero;
        Vector3 avgVel = Vector3.zero;

        //ALL RBS
        int numOfRb = 0;
        foreach (var item in m_JdController.bodyPartsList)
        {
            numOfRb++;
            velSum += item.rb.velocity;
        }

        avgVel = velSum / numOfRb;
        return avgVel;
    }

    /// <summary>
    /// Normalized value of the difference in actual speed vs goal walking speed.
    /// </summary>
    public float GetMatchingVelocityReward(Vector3 velocityGoal, Vector3 actualVelocity)
    {
        //distance between our actual velocity and goal velocity
        var velDeltaMagnitude = Mathf.Clamp(Vector3.Distance(actualVelocity, velocityGoal), 0, TargetWalkingSpeed);

        //return the value on a declining sigmoid shaped curve that decays from 1 to 0
        //This reward will approach 1 if it matches perfectly and approach zero as it deviates
        return Mathf.Pow(1 - Mathf.Pow(velDeltaMagnitude / TargetWalkingSpeed, 2), 2);
    }
}
