using Fusion;
using UnityEngine;

namespace SpellFlinger.PlayScene
{
    public struct NetworkInputData : INetworkInput
    {
        public const byte SHOOT = 1;
        public const byte JUMP = 2;

        public int XDirection;
        public int YDirection;
        public float YRotation;
        public NetworkButtons buttons;
    }
}