using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgentsExamples;
using System.Xml.Linq;

[RequireComponent(typeof(JointDriveController))]
public class TestJDScript : MonoBehaviour
{
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

    [Header("Joints")]
    public float m_UpperJointX = 0;
    public float m_MidJointX = 0;
    public float m_MidJointY = 0;
    public float m_FootJointX = 0;
    public float m_FootJointZ = 0;
    public float m_BackJointX = 0;

    [Header("Strength")]
    public float m_JointStrength = 0;

    JointDriveController m_JDC;

    void Start()
    {
        m_JDC = GetComponent<JointDriveController>();

        //m_JDC.SetupBodyPart(seat);
        m_JDC.SetupBodyPart(rest);
 
        m_JDC.SetupBodyPart(FRT);
        m_JDC.SetupBodyPart(FRL);
        m_JDC.SetupBodyPart(FRF);

        m_JDC.SetupBodyPart(FLT);
        m_JDC.SetupBodyPart(FLL);
        m_JDC.SetupBodyPart(FLF);
   
        m_JDC.SetupBodyPart(BRT);
        m_JDC.SetupBodyPart(BRL);
        m_JDC.SetupBodyPart(BRF);

        m_JDC.SetupBodyPart(BLT);
        m_JDC.SetupBodyPart(BLL);
        m_JDC.SetupBodyPart(BLF);
    }
    // Update is called once per frame
    void Update()
    {
        var bpDict = m_JDC.bodyPartsDict;

        bpDict[rest].SetJointTargetRotation(m_BackJointX, 0, 0);

        bpDict[FRT].SetJointTargetRotation(m_UpperJointX, 0, 0);
        bpDict[FRL].SetJointTargetRotation(m_MidJointX, m_MidJointY, 0);
        bpDict[FLT].SetJointTargetRotation(m_UpperJointX, 0, 0);
        bpDict[FLL].SetJointTargetRotation(m_MidJointX, m_MidJointY, 0);
        bpDict[BRT].SetJointTargetRotation(m_UpperJointX, 0, 0);
        bpDict[BRL].SetJointTargetRotation(m_MidJointX, m_MidJointY, 0);
        bpDict[BLT].SetJointTargetRotation(m_UpperJointX, 0, 0);
        bpDict[BLL].SetJointTargetRotation(m_MidJointX, m_MidJointY, 0);
        bpDict[BRF].SetJointTargetRotation(m_FootJointX, 0, m_FootJointZ);
        bpDict[BLF].SetJointTargetRotation(m_FootJointX, 0, m_FootJointZ);
        bpDict[FRF].SetJointTargetRotation(m_FootJointX, 0, m_FootJointZ);
        bpDict[FLF].SetJointTargetRotation(m_FootJointX, 0, m_FootJointZ);

        foreach (var key in bpDict.Keys )
        {
            bpDict[key].SetJointStrength(m_JointStrength);
        }
    }
}
