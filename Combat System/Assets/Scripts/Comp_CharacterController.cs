using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gowtham
{
    public class Comp_CharacterController : MonoBehaviour
    {
        private Animator _animator;
        private Comp_PlayerInputs _inputs;
        private Comp_CameraController _cameraController;

        private void Start()
        {
            _animator = GetComponent<Animator>();
            _inputs = GetComponent<Comp_PlayerInputs>();
            _cameraController = GetComponent<Comp_CameraController>();
        }

        private void Update()
        {
            Vector3 _moveInputVector = new Vector3(_inputs.MoveAxisRightRaw, 0, _inputs.MoveAxisRightRaw);
            Vector3 _cameraPlanarDirection = _cameraController.CameraPlanarDirection;
            Quaternion _cameraPlanarRotation = Quaternion.LookRotation(_cameraPlanarDirection);

            Debug.DrawLine(transform.position, transform.position + _moveInputVector, Color.green);
        }
    }
}

