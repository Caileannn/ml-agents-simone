using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;


namespace Unity.MLAgentsExamples
{
    [DisallowMultipleComponent]
    public class SwapModelOnContact : MonoBehaviour
    {
        [Header("List of Collision Types & Models")]
        public string m_BalanceSwap = "swap_Balance";
        public string m_WalkerSwap = "swap_Walker";
        public string m_SloperSwap = "swap_Sloper";
        public string m_GetupSwap = "swap_Getup";

        private ModelSwap m_ModelSwapper;
        private Agent m_Agent;

        void OnEnable()
        {
            m_ModelSwapper = GetComponentInParent<ModelSwap>();
            m_Agent = GetComponentInParent<Agent>();
        }
        private void OnTriggerEnter(Collider col)
        {
            
            if (col.gameObject.CompareTag(m_GetupSwap))
            {
                // Swap Model to this
                m_ModelSwapper.SwitchModel("Getup", m_Agent);
            }

            if (col.gameObject.CompareTag(m_WalkerSwap))
            {
                // Swap Model to this
                m_ModelSwapper.SwitchModel("Walker", m_Agent);
            }

            if (col.gameObject.CompareTag(m_BalanceSwap))
            {
                // Swap Model to this
                m_ModelSwapper.SwitchModel("Balance", m_Agent);
            }

            if (col.gameObject.CompareTag(m_SloperSwap))
            {
                // Swap Model to this
                m_ModelSwapper.SwitchModel("Sloper", m_Agent);
            }


        }

        void OnTriggerExit(Collider col)
        {
            m_ModelSwapper.SwitchModel(4, m_Agent);
        }
    }
}
