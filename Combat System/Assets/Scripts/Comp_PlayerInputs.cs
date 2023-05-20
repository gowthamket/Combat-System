using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gowtham
{
    [System.Serializable]
    public class Input
    {
        public KeyCode primary;
        public KeyCode alternate;

        public bool Pressed()
        {
            return UnityEngine.Input.GetKey(primary) || UnityEngine.Input.GetKey(alternate);
        }
        public bool PressedDown()
        {
            return UnityEngine.Input.GetKeyDown(primary) || UnityEngine.Input.GetKeyDown(alternate);
        }
        public bool PressedUp()
        {
            return UnityEngine.Input.GetKeyDown(primary) || UnityEngine.Input.GetKeyDown(alternate);
        }
    }

    public class Comp_PlayerInputs : MonoBehaviour
    {
        public Input Forward;
        public Input Backward;
        public Input Right;
        public Input Left;
        public Input Sprint;
        public Input Aim;
        public Input LockOn;
        public Input Attack;

        public int MoveAxisForwardRaw
        {
            get
            {
                if (Forward.Pressed() && Backward.Pressed()) { return 0; }
                else if (Forward.Pressed()) { return 1; }
                else if (Backward.Pressed()) { return -1; }
                else { return 0; }
            }
        }

        public int MoveAxisRightRaw
        {
            get
            {
                if (Forward.Pressed() && Backward.Pressed()) { return 0; }
                else if (Forward.Pressed()) { return 1; }
                else if (Backward.Pressed()) { return -1; }
                else { return 0; }
            }
        }

        public const string MouseXString = "Mouse X";
        public const string MouseYString = "Mouse Y";
        public const string MouseScrollString = "MouseScrollWheel";

        public static float MouseXInput { get => UnityEngine.Input.GetAxis(MouseXString); }
        public static float MouseYInput { get => UnityEngine.Input.GetAxis(MouseYString); }
        public static float MouseScrollInput { get => UnityEngine.Input.GetAxis(MouseScrollString); }
    }
}

