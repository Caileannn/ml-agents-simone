using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgentsExamples;
using Unity.MLAgents.Sensors;
using BodyPart = Unity.MLAgentsExamples.BodyPart;
using Random = UnityEngine.Random;
using static UnityEngine.GraphicsBuffer;


public enum Team
{
    Blue = 0,
    Purple = 1
}

public class AgentClass : Agent
{
    public enum Position
    {
        Attacker,
        Defender
    }


    [HideInInspector]
    public Team team;
    float m_BallTouched = 1f;
    public Position position;

    float m_Existential;


    [HideInInspector]
    public Rigidbody agentRb;
    AgentSettings m_AgentSettings;
    BehaviorParameters m_BehaviourParameters;
    public Vector3 initialPosition;
    public float rotationSign;

    EnvironmentParameters m_ResetParameters;

    //The indicator graphic gameobject that points towards the target
    JointDriveController m_JdController;

    //This will be used as a stabilized model space reference point for observations
    //Because ragdolls can move erratically during training, using a stabilized reference transform improves learning
    OrientationCubeController m_OrientationCube;

    [Header("Body Parts")] public Transform body;
    public Transform arml;
    public Transform armr;

    public Transform ball;

    public override void Initialize()
    {
        // (?)
        AgentEnvController envController = GetComponentInParent<AgentEnvController>();
        m_OrientationCube = GetComponentInChildren<OrientationCubeController>();

        if (envController != null)
        {
            m_Existential = 1f / envController.MaxEnvironmentSteps;
        }
        else
        {
            m_Existential = 1f / MaxStep;
        }

        // Setting the team for the Agent
        m_BehaviourParameters = gameObject.GetComponent<BehaviorParameters>();
        if (m_BehaviourParameters.TeamId == (int)Team.Blue)
        {
            team = Team.Blue;
            initialPosition = new Vector3(transform.position.x - 5f, .6f, transform.position.z);
            rotationSign = 1f;
        }
        else
        {
            team = Team.Purple;
            initialPosition = new Vector3(transform.position.x + 5f, .6f, transform.position.z);
            rotationSign = -1f;
        }

        if (position == Position.Attacker)
        {
            // Debug.Log("Set to Attacker");
        }
        if (position == Position.Defender)
        {
            // Debug.Log("Set to Defender");
        }

        m_AgentSettings = FindObjectOfType<AgentSettings>();

        m_ResetParameters = Academy.Instance.EnvironmentParameters;

        SetupAgent();
    }

    public void SetupAgent()
    {
        //Setup each body part
        m_JdController = GetComponent<JointDriveController>();
        m_JdController.SetupBodyPart(body);
        m_JdController.SetupBodyPart(arml);
        m_JdController.SetupBodyPart(armr);
    }

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

    public override void CollectObservations(VectorSensor sensor)
    {
        // Avg. Velocity of Agent
        // Avg. Body velocity compared to the cube
        // Distance from Ball
        foreach (var bodyPart in m_JdController.bodyPartsList)
        {
            CollectObservationBodyPart(bodyPart, sensor);
        }

    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        var bpDict = m_JdController.bodyPartsDict;
        var i = -1;

        var continuousActions = actions.ContinuousActions;
        bpDict[arml].SetJointTargetRotation(continuousActions[++i], continuousActions[++i], continuousActions[++i]);
        bpDict[armr].SetJointTargetRotation(continuousActions[++i], continuousActions[++i], continuousActions[++i]);

        //update joint strength settings
        bpDict[arml].SetJointStrength(continuousActions[++i]);
        bpDict[armr].SetJointStrength(continuousActions[++i]);
    }

    private void FixedUpdate()
    {
        UpdateOrientationObjects();
        // Calculate Rewards
   
        // Add Existential Reward
        if (position == Position.Attacker)
        {
            AddReward(-m_Existential);
            
        }
        if (position == Position.Defender)
        {
            AddReward(m_Existential);
            
        }
    }

    private void OnCollisionEnter(Collision c)
    {
        if (c.gameObject.CompareTag("ball"))
        {
            // Add a reward when agent touches ball
            // AddReward(m_BallTouched);
        }
    }

    public override void OnEpisodeBegin()
    {

        m_BallTouched = m_ResetParameters.GetWithDefault("ball_touched", 0);
        
        //Reset all of the body parts
        foreach (var bodyPart in m_JdController.bodyPartsDict.Values)
        {
            bodyPart.Reset(bodyPart);
        }
    }

    void UpdateOrientationObjects()
    {
        m_OrientationCube.UpdateOrientation(body, ball);
    }


}

