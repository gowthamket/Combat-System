using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Comp_Hitbox : MonoBehaviour, IHitDetector
{
    [SerializeField] private BoxCollider m_Collider;
    [SerializeField] private LayerMask m_layerMask;
    [SerializeField] private HurtboxMask m_hurtboxMask = HurtboxMask.Enemy;

    private float m_thickness = 0.025f;
    private IHitResponder m_hitResponder;

    public IHitResponder HitResponder { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    IHitResponder IHitDetector { get => m_hitResponder; set => m_hitResponder = value; }

    public void CheckHit()
    {
        Vector3 _scaledSize = new Vector3(
            m_Collider.size.x * transform.lossyScale.x,
            m_Collider.size.y * transform.lossyScale.y,
            m_Collider.size.z * transform.lossyScale.z);

        float _distance = _scaledSize.y - m_thickness;
        Vector3 _direction = transform.up;
        Vector3 _center = transform.TransformPoint(m_Collider.center);
        Vector3 _start = _center - _direction * (_distance / 2);
        Vector3 _halfExtents = new Vector3(_scaledSize.x, m_thickness, _scaledSize.z) / 2;
        Quaternion _orientation = transform.rotation;

        HitData _hitData = null;
        IHurtbox _hurtbox = null;
        RaycastHit[] _hits = Physics.BoxCastAll(_start, _halfExtents, _direction, _orientation, _distance, m_layerMask);
        foreach (RaycastHit _hit in _hits)
        {
            _hurtbox = _hit.collider.GetComponent<IHurtbox>();
            if (_hurtbox != null)
            {
                if (_hurtbox.Active)
                {
                    if (m_hurtboxMask.HasFlag((HurtboxMask)_hurtbox.Type))
                    {
                        //Generate Hitdata
                        _hitData = new HitData
                        {
                            damage = m_hitResponder == null ? 0 : m_hitResponder.Damage,
                            hitPoint = _hit.point == Vector3.zero ? _center : _hit.point,
                            hitNormal = _hit.normal,
                            hurtbox = _hurtbox,
                            hitDetector = this
                        };

                        //Validate + Response
                        if (_hitData.Validate())
                        {
                            _hitData.hitDetector.HitResponder?.Response(_hitData);
                            _hitData.hurtbox.HurtResponder?.Response(_hitData);
                        }
                    }
                    
                }
            }
        }
    }
}
