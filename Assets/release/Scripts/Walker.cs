using UnityEngine;
using Unity.MLAgents.Actuators;
using Unity.MLAgentsExamples;
using Unity.MLAgents.Sensors;
using Unity.MLAgents;
using BodyPart = Unity.MLAgentsExamples.BodyPart;
using Random = UnityEngine.Random;
using static UnityEngine.GraphicsBuffer;
using UnityEngine.UI;
//using System.Diagnostics;

public class Walker : Agent
{
    // Walking Speed
    [Header("Walk Speed")]
    [Range(0.1f, 10)]
    [SerializeField]
    private float m_TargetWalkingSpeed = 10;

    [Header("Joint Controls")]
    [Range(20000f, 60000f)]
    [SerializeField]
    private float m_Spring = 40000f;
    [Range(2500f, 7500f)]
    [SerializeField]
    private float m_Dampen = 5000f;
    [Range(10000f, 30000f)]
    [SerializeField]
    private float m_Force = 20000f;

    // Property
    public float MTargetWalkingSpeed
    {
        get { return m_TargetWalkingSpeed; }
        set { m_TargetWalkingSpeed = Mathf.Clamp(value, .1f, m_MaxWalkingSpeed); }
    }

    // The Max Walking Speed
    const float m_MaxWalkingSpeed = 10;

    // Randomise Walking Speed every Episode
    public bool rWalkSpeedEachEpisode;

    public bool earlyTraining;

    // Randomise Joint Forces
    [Header("Walker Joint Forces")]
    public bool rJointForcesEachEpisode;

    // Randomise Joint Forces
    [Header("Getup Training")]
    public bool getUpTraining;

    [Header("Balance Training")]
    public bool balanceTraining;

    // The direction the Agent will walk towards during training
    private Vector3 m_WorldDirectionToWalk = Vector3.right;

    // Specifies the Target the Agent will Walk towards
    [Header("Target to Walk Towards")]
    public Transform target;

    /* Orientation Cube Controller - a reference point for observations, as
     Ragdolls can be very erratic in their movements */
    OrientationCubeController m_OrientationCube;

    LockOrientation m_Raycast;



    // Joint Drive Controller - Controls the params of Joints
    JointDriveController m_JointDriveController;

    // Specifies all the Body Parts (Configurable Joints)
    [Header("Body Parts")]
    public Transform rest;
    public Transform seat;

    public Transform FLT;
    public Transform FLL;
    public Transform FLF;

    public Transform FRT;
    public Transform FRL;
    public Transform FRF;

    public Transform BRT;
    public Transform BRL;
    public Transform BRF;

    public Transform BLT;
    public Transform BLL;
    public Transform BLF;

    // Raycast for Ground Check & Height
    private float height;
    RaycastHit hit;

    // Setup
    public override void Initialize()
    {
        m_OrientationCube = GetComponentInChildren<OrientationCubeController>();
        m_JointDriveController = GetComponent<JointDriveController>();
        // m_Raycast = GetComponentInChildren<LockOrientation>();
       
        // Setup each Body Part
        m_JointDriveController.SetupBodyPart(seat);
        m_JointDriveController.SetupBodyPart(rest);

        m_JointDriveController.SetupBodyPart(FRT);
        m_JointDriveController.SetupBodyPart(FRL);
        m_JointDriveController.SetupBodyPart(FRF);

        m_JointDriveController.SetupBodyPart(FLT);
        m_JointDriveController.SetupBodyPart(FLL);
        m_JointDriveController.SetupBodyPart(FLF);

        m_JointDriveController.SetupBodyPart(BRT);
        m_JointDriveController.SetupBodyPart(BRL);
        m_JointDriveController.SetupBodyPart(BRF);

        m_JointDriveController.SetupBodyPart(BLT);
        m_JointDriveController.SetupBodyPart(BLL);
        m_JointDriveController.SetupBodyPart(BLF);
    }

    /* Loop over Body Parts & Reset them to inital conditions
     */
    public override void OnEpisodeBegin()
    {
        // Set values of Joint Controller for exploring emrgent behvaiours
        SetJointForces();

        if (rJointForcesEachEpisode)
        {
            // Apply Random Forces
            RandomJointForces();
        }

        foreach (var bodyPart in m_JointDriveController.bodyPartsDict.Values)
        {
            bodyPart.Reset(bodyPart);
        }

        // Apply Random Rotation to Seat
        if (getUpTraining) { seat.rotation = Quaternion.Euler(Random.Range(0.0f, 360f), Random.Range(0.0f, 360f), Random.Range(0.0f, 360f)); }
        else { seat.rotation = Quaternion.Euler(0f, Random.Range(0.0f, 360f), 0f);  }
        
        UpdateOrientationObject();

        // Set our Walking Speed goal
        MTargetWalkingSpeed =
            rWalkSpeedEachEpisode ? Random.Range(0.1f, m_MaxWalkingSpeed) : MTargetWalkingSpeed;

        
    }
    
    /* Add relevant information for each body part (observations)
     */
    public void CollectObservationsBP(BodyPart bp, VectorSensor sensor)
    {
        // Check if touching Ground
        sensor.AddObservation(bp.groundContact.touchingGround);

        // Get Velocities in Context of the Orientation Cube Space
        sensor.AddObservation(m_OrientationCube.transform.InverseTransformDirection(bp.rb.velocity));
        sensor.AddObservation(m_OrientationCube.transform.InverseTransformDirection(bp.rb.angularVelocity));

        // Gets Position relative to the Seat in Context of our Orientation Cube
        sensor.AddObservation(m_OrientationCube.transform.InverseTransformDirection(bp.rb.position - seat.position));


        if (bp.rb.transform != seat)
        {
            sensor.AddObservation(bp.rb.transform.localRotation);
            sensor.AddObservation(bp.currentStrength / m_JointDriveController.maxJointForceLimit);
        }
    }

    /* Main method to Collect Observations about environemt
     */
    public override void CollectObservations(VectorSensor sensor)
    {
        // Direction Agent is Facing
        var cubeForward = m_OrientationCube.transform.forward;

        // Velocity we want to Match
        var velocityGoal = cubeForward * MTargetWalkingSpeed;

        // Ragdolls AVG Velocity
        var avgVelocity = GetAvgVelocity();

        // Current Ragdoll Vecloty, Normalised (How far away is the goal vs. current vel)
        sensor.AddObservation(Vector3.Distance(velocityGoal, avgVelocity));

        // Avgerage Body Velocity Relative to Orientation Cube
        sensor.AddObservation(m_OrientationCube.transform.InverseTransformDirection(avgVelocity));

        // Velocity Goal Relative to the Cube
        sensor.AddObservation(m_OrientationCube.transform.InverseTransformDirection(velocityGoal));

        // Rotation Delta
        sensor.AddObservation(Quaternion.FromToRotation(seat.forward, cubeForward));

        // Position of target relative to Cube
        sensor.AddObservation(m_OrientationCube.transform.InverseTransformDirection(target.transform.position));
    
        foreach (var bodyPart in m_JointDriveController.bodyPartsList)
        {
            CollectObservationsBP(bodyPart, sensor);
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActionsOut = actionsOut.ContinuousActions;
        //continuousActionsOut[2] = Input.GetAxis("Horizontal");
        //continuousActionsOut[3] = Input.GetAxis("Vertical");
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        var bpDict = m_JointDriveController.bodyPartsDict;
        var element = -1;

        var continuousActions = actions.ContinuousActions;

        bpDict[rest].SetJointTargetRotation(continuousActions[++element], 0, 0);

        bpDict[FRT].SetJointTargetRotation(continuousActions[++element], 0, 0);
        bpDict[FRL].SetJointTargetRotation(continuousActions[++element], continuousActions[++element], 0);
        bpDict[FLT].SetJointTargetRotation(continuousActions[++element], 0, 0);
        bpDict[FLL].SetJointTargetRotation(continuousActions[++element], continuousActions[++element], 0);
        bpDict[BRT].SetJointTargetRotation(continuousActions[++element], 0, 0);
        bpDict[BRL].SetJointTargetRotation(continuousActions[++element], continuousActions[++element], 0);
        bpDict[BLT].SetJointTargetRotation(continuousActions[++element], 0, 0);
        bpDict[BLL].SetJointTargetRotation(continuousActions[++element], continuousActions[++element], 0);
        bpDict[BRF].SetJointTargetRotation(continuousActions[++element], 0, 0);
        bpDict[BLF].SetJointTargetRotation(continuousActions[++element], 0, 0);
        bpDict[FRF].SetJointTargetRotation(continuousActions[++element], 0, 0);
        bpDict[FLF].SetJointTargetRotation(continuousActions[++element], 0, 0);

        bpDict[rest].SetJointStrength(continuousActions[++element]);
        bpDict[FRT].SetJointStrength(continuousActions[++element]);
        bpDict[FRL].SetJointStrength(continuousActions[++element]);
        bpDict[FLT].SetJointStrength(continuousActions[++element]);
        bpDict[FLL].SetJointStrength(continuousActions[++element]);
        bpDict[BRT].SetJointStrength(continuousActions[++element]);
        bpDict[BRL].SetJointStrength(continuousActions[++element]);
        bpDict[BLT].SetJointStrength(continuousActions[++element]);
        bpDict[BLL].SetJointStrength(continuousActions[++element]);
        bpDict[BLF].SetJointStrength(continuousActions[++element]);
        bpDict[BRF].SetJointStrength(continuousActions[++element]);
        bpDict[FLF].SetJointStrength(continuousActions[++element]);
        bpDict[FRF].SetJointStrength(continuousActions[++element]);
    }

    void UpdateOrientationObject()
    {
        m_WorldDirectionToWalk = target.position - seat.position;
        m_OrientationCube.UpdateOrientation(seat, target);
        // m_Raycast.UpdateRotation(seat);
    }

    void FixedUpdate()
    {
        SetJointForces();
        UpdateOrientationObject();

        var cubeForward = m_OrientationCube.transform.forward;

        // Set reward for this step

        // (1) Match Target Speed
        var matchSpeedReward = GetMatchingVelocityReward(cubeForward * MTargetWalkingSpeed, GetAvgVelocity());

        // (2) Rotation alignment with target
        var lookAtTargetReward = (Vector3.Dot(cubeForward, seat.forward) + 1) * .5f;

        // (3) Distance from ground
        // var distFromGround = Mathf.Pow(Mathf.Clamp(Vector3.Distance(seat.transform.position, GameObject.Find("Ground").transform.position) + 0.2f, 0, 1), 2);

        if (earlyTraining)
        { //*Important* Forces movement towards target (penalize stationary swinging)
           matchSpeedReward = Vector3.Dot(GetAvgVelocity(), cubeForward);
           if (matchSpeedReward > 0) matchSpeedReward = GetMatchingVelocityReward(cubeForward * MTargetWalkingSpeed, GetAvgVelocity());
        }
        
        if (getUpTraining)
        {
            // Its Orientation Along Z-Axis, as it gets more upright, its gets closer to 1, else closer to 0
            // Distance from the seat to the ground, the same applies, higher = 1, lower = 0
            var zAngle = DeltaAngle(seat.eulerAngles.z);
            var xAngle = DeltaAngle(seat.eulerAngles.x);
            var distanceFromGround = DistanceFromGround();
            AddReward(zAngle * xAngle);
           
        } else if(balanceTraining)
        {
            var zAngle = DeltaAngle(seat.eulerAngles.z);
            var xAngle = DeltaAngle(seat.eulerAngles.x);
            AddReward(matchSpeedReward * lookAtTargetReward * (zAngle * xAngle));
            
        }
        else
        {
           AddReward(matchSpeedReward * lookAtTargetReward);
        }
    }

    

    Vector3 GetAvgVelocity()
    {
        Vector3 velSum = Vector3.zero;

        int rbCount = 0;
        foreach (var item in m_JointDriveController.bodyPartsList)
        {
            rbCount++;
            velSum += item.rb.velocity;
        }

        var avgVelocity = velSum / rbCount;
        return avgVelocity;
    }

    public float GetMatchingVelocityReward(Vector3 velGoal, Vector3 currentVel)
    {
        var velDeltaMag = Mathf.Clamp(Vector3.Distance(currentVel, velGoal), 0, MTargetWalkingSpeed);
        return Mathf.Pow(1 - Mathf.Pow(velDeltaMag / MTargetWalkingSpeed, 2), 2);
    }

    float DeltaAngle(float angle)
    {
        var currentZRot = angle;
        float zRotDist = Mathf.Abs(Mathf.DeltaAngle(0f, currentZRot));
        float normalizedDistance = 1f - Mathf.InverseLerp(0f, 180f, zRotDist);
        float expZDist = Mathf.Pow(normalizedDistance, 2);
        return expZDist;
    }

    float DistanceFromGround()
    {
        float normalizedValue = Mathf.Clamp01( (seat.position.y) / 3.0f);
        Debug.Log(seat.position.y/3.0f);
        // Debug.Log(normalizedValue);
        return Mathf.Pow(normalizedValue, 2);
    }

    void SetJointForces()
    {
        m_JointDriveController.maxJointSpring = m_Spring;
        m_JointDriveController.jointDampen = m_Dampen;
        m_JointDriveController.maxJointForceLimit = m_Force;

        try
        {
            var t_Force = GameObject.Find("t_Force").GetComponent<Text>();
            t_Force.text = m_Force.ToString();
            var t_Spring = GameObject.Find("t_Spring").GetComponent<Text>();
            t_Spring.text = m_Spring.ToString();
            var t_Dampen = GameObject.Find("t_Dampen").GetComponent<Text>();
            t_Dampen.text = m_Dampen.ToString();
        } catch
        {
            Debug.Log("Text UI cannot be found.");
        }
    }

    void RandomJointForces()
    {
        m_JointDriveController.jointDampen = Random.Range(2500, 7500);
        m_JointDriveController.maxJointSpring = Random.Range(20000, 60000);
        m_JointDriveController.maxJointForceLimit = Random.Range(10000, 30000);
    }
}
