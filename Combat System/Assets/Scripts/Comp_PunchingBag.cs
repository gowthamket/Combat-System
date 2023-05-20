using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Comp_PunchingBag : MonoBehaviour, ITargetable, IHurtResponder
{
    [SerializeField] private bool m_targetable = true;
    [SerializeField] private Transform m_targetTransform;
    [SerializeField] private Rigidbody m_rbBag;

    private List<Comp_Hurtbox> m_hurtboxes = new List<Comp_Hurtbox>();
    bool ITargetable.Targetable { get => m_targetTransform; }

    Transform ITargetable.TargetTransform { get => m_targetTransform; }

    private void Start()
    {
        m_hurtboxes = new List<Comp_Hurtbox>(GetComponentsInChildren<Comp_Hurtbox>());
        foreach (Comp_Hurtbox m_hurtbox in m_hurtboxes)
        {
            m_hurtbox.HurtResponder = this;
        }
    }

    bool IHurtResponder.CheckHit(HitData data)
    {
        return true;
    }

    void IHurtResponder.Response(HitData data)
    {
        Debug.Log("Hurt Response");
    }
}
