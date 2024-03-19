using UnityEngine;
using Unity.MLAgents.Actuators;
using Unity.MLAgentsExamples;
using Unity.MLAgents.Sensors;
using Unity.MLAgents;
using BodyPart = Unity.MLAgentsExamples.BodyPart;
using Random = UnityEngine.Random;
using static UnityEngine.GraphicsBuffer;
using UnityEngine.UI;
using Unity.VisualScripting;
using System.Collections.Generic;
using System.Xml.Linq;
using Unity.Burst.CompilerServices;
//using System.Diagnostics;

public class Walker : Agent
{

    

    public enum Brain
    {
        Walker,
        Getup,
        Climber,
        Balance,
        DMScrambler,
        Sitting,
        Treadmill
    }

    public Brain m_SelectedBrain;

    [Header("Walk Speed")]
    [Range(0.1f, 30)]
    [SerializeField]
    private float m_TargetWalkingSpeed = 30;

    public float MTargetWalkingSpeed
    {
        get { return m_TargetWalkingSpeed; }
        set { m_TargetWalkingSpeed = Mathf.Clamp(value, .1f, m_MaxWalkingSpeed); }
    }

    // The Max Walking Speed
    const float m_MaxWalkingSpeed = 30;

    [Header("Randomise")]
    // Randomise Walking Speed every Episode
    public bool rWalkSpeedEachEpisode;
    public bool m_RandomiseYRotation = true;
    public bool m_RandomiseXYZRotation = false;

    [Header("Swap Model Settings")]
    public bool m_ModelSwap = false;
    public bool m_ProximitySwapper = false;
    public bool m_SwitchModelAfterFalling = false;

    [Header("DM Monitor")]
    public int m_StepCountAtLastMeter = 0;
    public int m_LastXPosition = 0;
    private Terrain m_Terrain;

    [HideInInspector]
    public bool m_FinishedSwap = false;

    [HideInInspector]
    public ModelSwap m_ModelSwapper;

    // The direction the Agent will walk towards during training
    private Vector3 m_WorldDirectionToWalk = Vector3.right;

    // Specifies the Target the Agent will Walk towards
    [Header("Target to Walk Towards")]
    public Transform target;

    /* Orientation Cube Controller - a reference point for observations, as
     Ragdolls can be very erratic in their movements */
    OrientationCubeController m_OrientationCube;

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

    // Setup
    public override void Initialize()
    {
        m_OrientationCube = GetComponentInChildren<OrientationCubeController>();
        m_JointDriveController = GetComponent<JointDriveController>();
        m_ModelSwapper = GetComponent<ModelSwap>();

        var parent = gameObject.transform.parent;

        try{
            m_Terrain = parent.GetComponentInChildren<Terrain>();
        }catch{
            m_Terrain = null; 
        }

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
        if (m_SelectedBrain == Brain.DMScrambler)
        {
            //this.GetComponent<DMTerrain>().Reset();
        }
        
        foreach (var bodyPart in m_JointDriveController.bodyPartsDict.Values)
        {
            bodyPart.Reset(bodyPart);
        }

        // Apply Random Rotation to Seat
        if (m_SelectedBrain == Brain.Getup) { if(m_RandomiseXYZRotation) seat.rotation = Quaternion.Euler(Random.Range(0.0f, 360f), Random.Range(0.0f, 360f), Random.Range(0.0f, 360f)); }
        else { if (m_RandomiseYRotation) seat.rotation = Quaternion.Euler(0f, Random.Range(0.0f, 360f), 0f);  }
        
        UpdateOrientationObject();

        // Set our Walking Speed goal
        MTargetWalkingSpeed =
            rWalkSpeedEachEpisode ? Random.Range(0.1f, m_MaxWalkingSpeed) : MTargetWalkingSpeed;

        // Check if model swapper is enabled, and set model if so
        if (m_ModelSwap)
        {
            m_ModelSwapper.SwitchModel("24_Walker", this);
        }

        m_LastXPosition = (int)GetAverageXPositionFeet();
        
    }
    
    /* Add relevant information for each body part (observations)
     */
    public void CollectObservationsBP(BodyPart bp, VectorSensor sensor)
    {
        // Check if touching Ground
        sensor.AddObservation(bp.groundContact.touchingGround);
        sensor.AddObservation(bp.groundContact.touchingStairs);
        sensor.AddObservation(bp.groundContact.touchingObject);

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

        // Distance of the target relative to the Cube
        //sensor.AddObservation(Vector3.Distance(m_OrientationCube.transform.position, target.transform.position));
    
        foreach (var bodyPart in m_JointDriveController.bodyPartsList)
        {
            CollectObservationsBP(bodyPart, sensor);
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActionsOut = actionsOut.ContinuousActions;
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
    }

    void FixedUpdate()
    {
        // Check if model swapper is on
        if (m_ModelSwap)
        {
            SwitchModelAfterFalling();
        }

        UpdateOrientationObject();

        var cubeForward = m_OrientationCube.transform.forward;

        var matchSpeedReward = GetMatchingVelocityReward(cubeForward * MTargetWalkingSpeed, GetAvgVelocity());

        var lookAtTargetReward = (Vector3.Dot(cubeForward, seat.forward) + 1) * .5f;

        // (3) Distance from ground
        // var distFromGround = Mathf.Pow(Mathf.Clamp(Vector3.Distance(seat.transform.position, GameObject.Find("Ground").transform.position) + 0.2f, 0, 1), 2);
        
        if (m_SelectedBrain == Brain.Getup)
        {

            Vector2 deltaAngle = GetAngleDeltaXZ();
            //Debug.Log($"{Mathf.Pow((deltaAngle.x * deltaAngle.y), 2)}");
            AddReward( Mathf.Pow((deltaAngle.x * deltaAngle.y), 2) * 0.02f );
           
        } 
        else if(m_SelectedBrain == Brain.Balance)
        {
            // Sets reward for the agent based on its stableness, while moving towards a target
            Vector2 deltaAngle = GetAngleDeltaXZ();
            AddReward(matchSpeedReward * lookAtTargetReward * (deltaAngle.x * deltaAngle.y));
            
        }
        else if(m_SelectedBrain == Brain.DMScrambler)
        {
            // Normalised Velocity in a certain direciton.
            //var seatVelocity = GetAvgVelocitySeat();
            //float normalisedVelcoity = Mathf.Clamp(GetNormalizedVelocity(seatVelocity).z, 0f, 1f);
            //AddReward(normalisedVelcoity);
            //AddReward(matchSpeedReward * lookAtTargetReward);
            // If it hasnt moved forward in a certain amount of steps, end the episode.
            // (1) Get X Position of the Foot, or Avgerge of all Feet

            AddReward(matchSpeedReward * lookAtTargetReward * DistanceFromTarget(20f));

            float feetXPosition = GetAverageXPositionFeet();
            int newXPosition = (int)feetXPosition;



            // (2) Compare step count with highest X position
            if (newXPosition > m_LastXPosition)
            {
                m_LastXPosition = newXPosition;
                m_StepCountAtLastMeter = this.StepCount;
            }

            // If the agent goes 1000 steps w/o making any progress, we will end the episode.
            if (this.StepCount - m_StepCountAtLastMeter >= (1000))
            {
                SetReward(-1f);
                EndEpisode();
            }

            //AddReward(-0.002f);

            //Debug.Log(DistanceFromTarget(20f));
        }
       else if(m_SelectedBrain == Brain.Walker)
        {
            AddReward(matchSpeedReward * lookAtTargetReward);
        }
        else if(m_SelectedBrain == Brain.Climber)
        {
            //AddReward(matchSpeedReward * lookAtTargetReward * DistanceFromTarget(20f));
            AddReward(-0.002f);
        }
        else if(m_SelectedBrain == Brain.Sitting)
        {
            var height = CheckHeightRaycast();
            var clampedHeight = Mathf.Pow( (1 - height), 2);
            //Debug.Log($"Clamped Height: {Mathf.Pow(clampedHeight,2)}");
            AddReward(clampedHeight);
        }
        else if(m_SelectedBrain == Brain.Treadmill) 
        {
            //Debug.Log(DistanceFromTarget(5f));
            AddReward(matchSpeedReward * lookAtTargetReward * DistanceFromTarget(5f));
        }
    }

    // Use regular update to listen for keypresses
    void Update()
    {
        if (m_ModelSwap)
        {
            InputSwitchModel();
        }
        
    }

    private void InputSwitchModel()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            m_ModelSwapper.SwitchModel("24_Walker", this);
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            m_ModelSwapper.SwitchModel("Stairs", this);
        }
    }

    private void SwitchModelAfterFalling()
    {
        if (m_SwitchModelAfterFalling)
        {
            // If on, check its rotation, and if certain parts are touching the floor
            Vector2 deltaAngle = GetAngleDeltaXZ();

            if ((deltaAngle.y < 0.5 || deltaAngle.x < 0.5) && !m_FinishedSwap)
            {
                // Swap Model
                m_FinishedSwap = true;
                m_ModelSwapper.SwitchModel("Getup", this);
            }
            else if (deltaAngle.y > 0.8 && deltaAngle.x > 0.8 && m_FinishedSwap)
            {
                // Swap to Original Model
                m_FinishedSwap = false;
                m_ModelSwapper.SwitchModel(4, this);
            }

        }
    }
    float DistanceFromTarget(float maxDistance) 
    {
        float dist =  Vector3.Distance(seat.transform.position, target.transform.position);
        float normalisedValue = 1- Mathf.InverseLerp(0f, maxDistance, dist);
        return Mathf.Pow(normalisedValue, 2);
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

    Vector3 GetAvgVelocitySeat()
    {
        Vector3 velSum = Vector3.zero;

        
        foreach (var item in m_JointDriveController.bodyPartsList)
        {
            if(item.rb.transform == seat)
            {
                velSum = item.rb.velocity;
                break;
            }
        }

        return velSum;
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

    Vector2 GetAngleDeltaXZ()
    {
        return new Vector2(DeltaAngle(seat.eulerAngles.x), DeltaAngle(seat.eulerAngles.z));
    }

    float CheckHeightRaycast()
    {
        RaycastHit hit;

        if (Physics.Raycast(seat.transform.position, transform.TransformDirection(Vector3.down), out hit, Mathf.Infinity))
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.red);
            return hit.distance / 2;
        }
        else
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward), Color.blue);
            return 1;
        }
    }

    float GetAverageXPositionFeet()
    {
        float average = (FLF.position.z + FRF.position.z + BLF.position.z + BRF.position.z) / 4.0f;
        return average;
    }

    Vector3 GetNormalizedVelocity(Vector3 metersPerSecond)
    {
        var maxMetersPerSecond = m_Terrain.terrainData.bounds.size
            / this.MaxStep
            / Time.fixedDeltaTime;

        var maxXZ = Mathf.Max(maxMetersPerSecond.x, maxMetersPerSecond.z);
        maxMetersPerSecond.x = maxXZ;
        maxMetersPerSecond.z = maxXZ;
        maxMetersPerSecond.y = 53; // override with
        float x = metersPerSecond.x / maxMetersPerSecond.x;
        float y = metersPerSecond.y / maxMetersPerSecond.y;
        float z = metersPerSecond.z / maxMetersPerSecond.z;
        // clamp result
        x = Mathf.Clamp(x, -1f, 1f);
        y = Mathf.Clamp(y, -1f, 1f);
        z = Mathf.Clamp(z, -1f, 1f);
        Vector3 normalizedVelocity = new Vector3(x, y, z);
        return normalizedVelocity;
    }
}
