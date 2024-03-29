﻿using Fusion;
using UnityEngine;

namespace CoreGame
{
    [System.Flags]
    public enum ButtonFlag
    {
        FORWARD = 1 << 0,
        BACKWARD = 1 << 1,
        LEFT = 1 << 2,
        RIGHT = 1 << 3,
        RESPAWN = 1 << 4,

        MOUSE_DOWN = 1 << 5,
        MOUSE_UP = 1 << 6,
    }

    public struct InputData : INetworkInput
    {
        public ButtonFlag ButtonFlags;
        public Vector3 Direction;
        public bool GetButton(ButtonFlag button)
        {
            return (ButtonFlags & button) == button;
        }
    }
        
}