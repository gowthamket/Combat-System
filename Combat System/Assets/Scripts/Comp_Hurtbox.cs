using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Comp_Hurtbox : MonoBehaviour, IHurtbox
{
    [SerializeField] private bool m_active = true;
    [SerializeField] private GameObject m_owner = null;
    [SerializeField] private HurtboxType m_hurtboxType = HurtboxType.Enemy;
    private IHurtResponder m_HurtResponder;


    public bool Active { get => m_active; }

    public GameObject Owner { get => m_owner; }

    public Transform Transform { get => Transform; }

    public HurtboxType Type { get => m_hurtboxType; }

    public IHurtResponder HurtResponder { get => m_HurtResponder; set => m_HurtResponder = value; }

    public bool CheckHit(HitData hitData)
    {
        if (m_HurtResponder == null)
        {
            Debug.Log("No Responder");
        }
        return true;

    }
}
