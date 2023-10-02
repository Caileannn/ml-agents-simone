using UnityEngine;
using Unity.MLAgents.Actuators;
using Unity.MLAgentsExamples;
using Unity.MLAgents.Sensors;
using Unity.MLAgents;
using BodyPart = Unity.MLAgentsExamples.BodyPart;
using Random = UnityEngine.Random;

public class RandomAct : MonoBehaviour
{

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

    // Start is called before the first frame update
    void Start()
    {
        m_JointDriveController = GetComponent<JointDriveController>();

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

        foreach (var item in m_JointDriveController.bodyPartsList)
        {
            Debug.Log(item.joint);
        }
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        RandomActuations();
    }


    private void RandomActuations() 
    {
        var bpDict = m_JointDriveController.bodyPartsList;
        var element = 0;

        float continuousActions = 0f;

        foreach (var bp in bpDict)
        {
            continuousActions = (Mathf.PerlinNoise1D(Time.realtimeSinceStartup + element) * 2f) - 1f;
            if (bp.rb.transform != seat)
            {
                bp.SetJointTargetRotation(continuousActions, 0, 0);
                Debug.Log(continuousActions);
                element++;
            }
           
        }

        foreach (var bp in bpDict)
        {
            continuousActions = (Mathf.PerlinNoise1D(Time.realtimeSinceStartup + element) * 2f) - 1f;
            if (bp.rb.transform != seat)
            {
                bp.SetJointStrength(continuousActions);
                element++;
            }
        }
    }

}
