using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gowtham
{
    public class Comp_CharacterController : MonoBehaviour, IHitResponder
    {
        [Header("Movement")]
        [SerializeField] private float _walkspeed = 2f;
        [SerializeField] private float _runspeed = 6f;
        [SerializeField] private float _sprintspeed = 8f;

        [Header("Sharpness")]
        [SerializeField] private float _moveSharpness = 10f;
        [SerializeField] private float _rotationSharpness = 10f;

        [Header("Attacking")]
        [SerializeField] private int _damage = 10;
        [SerializeField] private Comp_Hitbox _hitboxLeftFist;
        [SerializeField] private GameObject _hitSparkPrefab;
 
        private Animator _animator;
        private Comp_PlayerInputs _inputs;
        private Comp_SMBEventCurator _eventCurator;
        private Comp_CameraController _cameraController;

        private bool _strafing;
        private bool _sprinting;
        private float _strafeParameter;
        private Vector3 _strafeParametersXZ;

        private float _targetSpeed;
        private Quaternion _targetRotation;

        private float _newSpeed;
        private Quaternion _newRotation;
        private Vector3 _newVelocity;

        private bool _jabbing;
        private bool _inAnimation;
        private Vector3 _animatorVelocity;
        private Quaternion _animatorDeltaRotation;
        private string _animJab = "Base Layer, Boxing Jab Left";
        private List<GameObject> _objectsHit = new List<GameObject> ();

        int IHitResponder.Damage { get => _damage; }

        private void Start()
        {
            _animator = GetComponent<Animator>();
            _inputs = GetComponent<Comp_PlayerInputs>();
            _eventCurator = GetComponent<Comp_SMBEventCurator>();
            _cameraController = GetComponent<Comp_CameraController>();

            //_animator.applyRootMotion = false;
            _eventCurator.Event.AddListener(OnSMBEvent);
        }

        private void Update()
        {
            Vector3 _moveInputVector = new Vector3(_inputs.MoveAxisRightRaw, 0, _inputs.MoveAxisRightRaw).normalized;
            Vector3 _cameraPlanarDirection = _cameraController.CameraPlanarDirection;
            Quaternion _cameraPlanarRotation = Quaternion.LookRotation(_cameraPlanarDirection);

           
            Vector3 _moveInputVectorOriented = _cameraPlanarRotation * _moveInputVector;

            _strafing = _cameraController.LockedOn;
            if (_strafing)
            {
                _sprinting = _inputs.Sprint.PressedDown() && (_moveInputVector != Vector3.zero);
                _strafing = _inputs.Aim.PressedDown() && !_sprinting;
            }
            else
            {
                _sprinting = _inputs.Sprint.Pressed() && (_moveInputVector != Vector3.zero);
                _strafing = _inputs.Aim.PressedDown();
            }

            if (_sprinting)
            {
                _cameraController.ToggleLockOn(false);
            }

            //Move speed
            if (_sprinting)
            {
                _targetSpeed = _moveInputVector != Vector3.zero ? _sprintspeed : 0;
            }
            else if (_strafing)
            {
                _targetSpeed = _moveInputVector != Vector3.zero ? _walkspeed : 0;
            }
            else
            {
                _targetSpeed = _moveInputVector != Vector3.zero ? _runspeed : 0;
            }
            _newSpeed = Mathf.Lerp(_newSpeed, _targetSpeed, Time.deltaTime * _moveSharpness);

            //Velocity
            if (_inAnimation)
            {
                _newVelocity = _animatorVelocity;
            }
            else
            {
                _newVelocity = _moveInputVectorOriented * _newSpeed;
            }
            transform.Translate(_newVelocity * Time.deltaTime, Space.World);

            if (!_inAnimation)
            {
                _newVelocity = _moveInputVectorOriented * _newSpeed;
                _targetSpeed = _moveInputVector != Vector3.zero ? _runspeed : 0;
            }

            //Rotation
            if (_inAnimation)
            {
                transform.rotation *= _animatorDeltaRotation;
            }
            else if (_strafing)
            {
                Vector3 _toTarget = _cameraController.Target.TargetTransform.position - transform.position;
                Vector3 _planarToTarget = Vector3.ProjectOnPlane(_toTarget, Vector3.up);

                _targetRotation = Quaternion.LookRotation(_planarToTarget);
                _newRotation = Quaternion.Slerp(transform.rotation, _targetRotation, Time.deltaTime * _rotationSharpness);
                transform.rotation = _newRotation;
            }
            else if (_targetSpeed != 0)
            {
                _targetRotation = Quaternion.LookRotation(_moveInputVector);
                _newRotation = Quaternion.Slerp(transform.rotation, _targetRotation, Time.deltaTime * _rotationSharpness);
                transform.rotation = _targetRotation;
            }

            //Animations
            if (_strafing)
            {
                _strafeParameter = Mathf.Clamp01(_strafeParameter + (Time.deltaTime * 4));
                _strafeParametersXZ = Vector3.Lerp(_strafeParametersXZ, _moveInputVector * _newSpeed, _moveSharpness * Time.deltaTime);
            }
            else
            {
                _strafeParameter = Mathf.Clamp01(_strafeParameter - (Time.deltaTime * 4));
                _strafeParametersXZ = Vector3.Lerp(_strafeParametersXZ, Vector3.forward * _newSpeed, _moveSharpness * Time.deltaTime);
            }

            _animator.SetFloat("Strafing", _strafeParameter);
            _animator.SetFloat("StrafeX", Mathf.Round(_strafeParametersXZ.x * 100f) / 100f);
            _animator.SetFloat("StrafeZ", Mathf.Round(_strafeParametersXZ.z * 100f) / 100f);

            //Request Lock On
            if (_inputs.LockOn.PressedDown())
            {
                _cameraController.ToggleLockOn(_cameraController.LockedOn);
            }

            //Jab Initiation
            if (!_inAnimation)
            {
                if (_inputs.Attack.PressedDown())
                {
                    _inAnimation = true;
                    _animator.CrossFadeInFixedTime(_animJab, 0.1f, 0, 0);
                }
            }

            if (_jabbing)
            {
                _hitboxLeftFist.CheckHit();
            }
        }

        private void OnAnimatorMove()
        {
            if (_inAnimation)
            {
                _animatorVelocity = _animator.velocity;
                _animatorDeltaRotation = _animator.deltaRotation;
            }
        }

        private void OnSMBEvent(string eventName)
        {
            switch (eventName)
            {
                case "JabStart":
                    _objectsHit.Clear();
                    _jabbing = true;
                    break;

                case "JabbingEnd":
                    _jabbing = false;
                    break;
                case "AnimationEnd":
                    _inAnimation = false;
                    break;
            }
        }

        bool IHitResponder.CheckHit(HitData data)
        {
            if (data.hurtbox.Owner == gameObject)
            {
                return false;
            }
            else if (_objectsHit.Contains(data.hurtbox.Owner))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        void IHitResponder.Response(HitData data)
        {
            _objectsHit.Add(data.hurtbox.Owner);
            Instantiate(_hitSparkPrefab, data.hitPoint, Quaternion.identity);
        }
    }
}

