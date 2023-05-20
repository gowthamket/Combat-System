using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Gowtham
{
    public class Comp_CameraController : MonoBehaviour
    {
        [Header("Framing")]
        [SerializeField] private Camera _camera = null;
        [SerializeField] private Transform _followTransform = null;
        [SerializeField] private Vector2 _framingNormal = new Vector3(0,0,0);

        [Header("Distance")]
        [SerializeField] private float _zoomSpeed = 10f;
        [SerializeField] private float _defaultDistance = 5f;
        [SerializeField] private float _minDistance = 0f;
        [SerializeField] private float _maxDistance = 10f;

        [Header("Rotation")]
        [SerializeField] private bool _invertX = false;
        [SerializeField] private bool _invertY = false;
        [SerializeField] private float _rotationSharpness = 25f;
        [SerializeField] [Range(-90, 90)] private float _minVerticalAngle = -90;
        [SerializeField] [Range(-90, 90)] private float _maxVerticalAngle = 90;
        [SerializeField] [Range(-90, 90)] private float _defaultVerticalAngle = 20f;

        [Header("Obstructions")]
        [SerializeField] private float _checkRadius = 0.2f;
        [SerializeField] private LayerMask _obstructionLayers;
        private List<Collider> _ignoreColliders = new List<Collider> ();

        [Header("Lock On")]
        [SerializeField] private float _lockOnLossTime = 15;
        [SerializeField] private float _lockOnDistance = 15;
        [SerializeField] private LayerMask _lockOnLayers = -1;
        [SerializeField] private Vector3 _lockOnFraming = Vector3.zero;
        [SerializeField, Range(1, 179)] private float _lockOnFOV = 40;

        //[SerializeField] private bool _lockedOn;
        //[SerializeField] private Transform _target;

        public bool LockedOn { get => _lockedOn; }
        public ITargetable Target { get => _target; }

        public Vector3 CameraPlanarDirection { get => _planarDirection; }

        //Privates
        private float _fovNormal;
        private float _framingLerp;
        private Vector3 _planarDirection; //Cameras forward on the x,z plane
        private Quaternion _targetRotation;
        private float _targetVerticalAngle;
        private float _targetDistance;
        private Vector3 _targetPosition;

        private Vector3 _newPosition;
        private Quaternion _newRotation;

        private bool _lockedOn;
        private float _lockOnLossTimeCurrent;
        private ITargetable _target;

        private void OnValidate()
        {
            _defaultDistance = Mathf.Clamp(_defaultDistance, _minDistance, _maxDistance);
            _defaultVerticalAngle = Mathf.Clamp(_defaultVerticalAngle, _minVerticalAngle, _maxVerticalAngle);
        }

        private void Start()
        {
            //Ignore the players colliders
            _ignoreColliders.AddRange(GetComponentsInChildren<Collider>());

            //Important
            _fovNormal = _camera.fieldOfView;
            _planarDirection = _followTransform.forward;

            //Calculate Targets
            _targetDistance = _defaultDistance;

            Cursor.lockState = CursorLockMode.Locked;   
        }

        private void Update()
        {
            if (Cursor.lockState != CursorLockMode.Locked)
            {
                return;
            }

            //Handle Inputs
            float _zoom = -Comp_PlayerInputs.MouseScrollInput * _zoomSpeed;
            float _mouseX = Comp_PlayerInputs.MouseXInput;
            float _mouseY = Comp_PlayerInputs.MouseYInput;

            if (_invertX) { _mouseX *= -1f; }
            if (_invertY) { _mouseY *= -1f; }

            //Framing
            Vector3 _farming = Vector3.Lerp(_framingNormal, _lockOnFraming, _framingLerp);
            Vector3 _focusPosition = _followTransform.TransformDirection(_framingNormal);

            float _fov = Mathf.Lerp(_fovNormal, _lockOnFOV, _framingLerp);
            _camera.fieldOfView = _fov;

            if (_lockedOn && _target != null)
            {
                Vector3 _camToTarget = Target.TargetTransform.position - _camera.transform.position;
                Vector3 _planarCamToTarget = Vector3.ProjectOnPlane(_camToTarget, Vector3.up);
                Quaternion _lookRotation = Quaternion.LookRotation(_camToTarget, Vector3.up);

                _framingLerp = Mathf.Clamp01(_framingLerp + Time.deltaTime * 4);
                _planarDirection = _planarCamToTarget != Vector3.zero ? _planarCamToTarget.normalized : _planarDirection;
                _targetDistance = Mathf.Clamp(_targetDistance + _zoom, _minDistance, _maxDistance);
                _targetVerticalAngle = Mathf.Clamp(_lookRotation.eulerAngles.x, _minVerticalAngle, _maxVerticalAngle);
            }
            else
            {
                _framingLerp = Mathf.Clamp01(_framingLerp - Time.deltaTime * 4);
                _planarDirection = Quaternion.Euler(0, _mouseX, 0) * _planarDirection;
                _targetDistance = Mathf.Clamp(_targetDistance + _zoom, _minDistance, _maxDistance);
                _targetVerticalAngle = Mathf.Clamp(_targetVerticalAngle + _mouseY, _minVerticalAngle, _maxVerticalAngle);
            }

            //_planarDirection = Quaternion.Euler(0, _mouseX, 0) * _planarDirection;
            //_targetVerticalAngle = Mathf.Clamp(_targetVerticalAngle + _mouseY, _minVerticalAngle, _maxVerticalAngle);

            Debug.DrawLine(_camera.transform.position, _camera.transform.position + _planarDirection, Color.red);

            //Handle Obstructions (affects target distance)
            float _smallestDistance = _targetDistance;
            RaycastHit[] _hits = Physics.SphereCastAll(_focusPosition, _checkRadius, _targetRotation * -Vector3.forward, _targetDistance, _obstructionLayers);
            if (_hits.Length != 0)
            {
                foreach (RaycastHit hit in _hits)
                {
                    if (!_ignoreColliders.Contains(hit.collider))
                    {
                        if (hit.distance < _smallestDistance)
                        {
                            _smallestDistance = hit.distance;
                        }
                    }
                }
            }

            //Final Targets
            _targetRotation = Quaternion.LookRotation(_planarDirection) * Quaternion.Euler(_targetVerticalAngle, 0, 0);
            _targetPosition = _followTransform.position - (_targetRotation * Vector3.forward) * _targetDistance;

            //Handle Smoothing
            _newRotation = Quaternion.Slerp(_camera.transform.rotation, _targetRotation, Time.deltaTime * _rotationSharpness);
            _newPosition = Vector3.Lerp(_camera.transform.position, _targetPosition, Time.deltaTime * _rotationSharpness);  

            //Apply
            _camera.transform.position = _newPosition;
            _camera.transform.rotation = _newRotation;
            if (_lockedOn && _target != null)
            {
                bool _valid = 
                    _target.Targetable && 
                    InDistance(_target) &&
                    InScreen(_target) &&
                    NotBlocked(_target);

                if (_valid) { _lockOnLossTimeCurrent = 0; }
                else { _lockOnLossTimeCurrent = Mathf.Clamp(_lockOnLossTimeCurrent + Time.deltaTime, 0, _lockOnLossTime); }
            }
            //_camera.transform.rotation = _targetRotation;
        }

        public void ToggleLockOn(bool toggle)
        {
            //Early out
            if (toggle == _lockedOn)
            {
                return;
            }

            //Toggle
            _lockedOn = !_lockedOn;

            //Find a lock on target
            if (_lockedOn)
            {
                //Filter targetables
                List<ITargetable> _targetables = new List<ITargetable>();
                Collider[] _colliders = Physics.OverlapSphere(transform.position, _lockOnDistance, _lockOnLayers);
                foreach (Collider _collider in _colliders)
                {
                    ITargetable _targetable = _collider.GetComponent<ITargetable>();
                    if (_targetable != null)
                    {
                        if (_targetable.Targetable)
                        {
                            if (InScreen(_targetable))
                            {
                                if (NotBlocked(_targetable))
                                {
                                    _targetables.Add(_targetable);
                                }
                            }
                        }
                    }
                }
                //Find closest targetal to screen center
                float _hypotenuse;
                float _smallestHypotenuse = Mathf.Infinity;
                ITargetable _closestTargetable = null;
                foreach (ITargetable _targetable in _targetables)
                {
                    _hypotenuse = CalculateHypotenuse(_targetable.TargetTransform.position);
                    if (_smallestHypotenuse > _hypotenuse)
                    {
                        _closestTargetable = _targetable;
                    }
                }

                //Final
                _target = _closestTargetable;
                _lockedOn = _closestTargetable != null; 
            }
        }

        private bool InDistance(ITargetable _targetable)
        {
            float _distance = Vector3.Distance(transform.position, _targetable.TargetTransform.position);
            return _distance <= _lockOnDistance;
        }

        private bool InScreen(ITargetable targetable)
        {
            Vector3 _viewPortPosition = _camera.WorldToViewportPoint(_target.TargetTransform.position);

            if (!(_viewPortPosition.x > 0) || !(_viewPortPosition.x < 1)) { return false; } 
            if (!(_viewPortPosition.y > 0) || !(_viewPortPosition.y < 1)) { return false; }
            if (!(_viewPortPosition.z > 0) || !(_viewPortPosition.z < 1)) { return false; }

            return true;
        }

        private bool NotBlocked(ITargetable targetable)
        {
            Vector3 _origin = _camera.transform.position;
            Vector3 _direction = targetable.TargetTransform.position - _origin;

            float _radius = 0.15f;
            float _distance = _direction.magnitude;
            bool _notBlocked = !Physics.SphereCast(_origin, _radius, _direction, out RaycastHit _hit, _distance, _obstructionLayers);

            return _notBlocked;
        }

        private float CalculateHypotenuse(Vector3 position)
        {
            float _screenCenterX = _camera.pixelWidth / 2;
            float _screenCenterY = _camera.pixelHeight / 2;

            Vector3 _screenPosition = _camera.WorldToScreenPoint(position);
            float _xDelta = _screenCenterX - _screenPosition.x;
            float _yDelta = _screenCenterY - _screenPosition.y;
            float _hypotenuse = Mathf.Sqrt(Mathf.Pow(_xDelta, 2) + Mathf.Pow(_yDelta, 2));

            return _hypotenuse;
        }
    }
}

