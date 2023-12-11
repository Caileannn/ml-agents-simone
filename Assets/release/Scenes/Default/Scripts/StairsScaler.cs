using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;


public class StairsScaler : MonoBehaviour
{
    // Start is called before the first frame update

    [Header("Scale Y")]
    public float m_ScaleY = 1;
    void Start()
    {
        // Set the scale and position of each layer
        AlterYScale();
        
    }

    // Update is called once per frame
    void Update()
    {
        AlterYScale();
    }

    private void AlterYScale()
    {
        var allKids = this.GetComponentInChildren<Transform>();
        var childCout = 0;
        foreach (Transform t in allKids)
        {
            t.localScale = new Vector3(t.lossyScale.x, m_ScaleY, t.lossyScale.z);
            t.localPosition = new Vector3(t.localPosition.x, ((m_ScaleY * childCout) + (m_ScaleY * 0.5f)) , t.localPosition.z);
            childCout++;
        }
    }

    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.red;
    //    Gizmos.DrawWireCube(transform.position, transform.localScale * 25);
    //}
}
