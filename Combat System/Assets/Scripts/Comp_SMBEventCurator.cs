using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Gowtham
{
    public class Comp_SMBEventCurator : MonoBehaviour
    {
        [SerializeField] private bool m_debug;
        [SerializeField] private UnityEvent<string> m_event = new UnityEvent<string>();
        public UnityEvent<string> Event { get => m_event; }

        private void Awake()
        {
            
        }

        private void OnSMBEvent(string eventName)
        {
            if (m_debug)
            {
                Debug.Log(eventName);
            }
        }
    }
}

