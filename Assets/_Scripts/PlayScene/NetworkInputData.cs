using Fusion;
using UnityEngine;

namespace SpellFlinger.PlayScene
{
    public struct NetworkInputData : INetworkInput
    {
        public int XDirection;
        public int YDirection;
        public bool Jump;
        public float YRotation;
        public bool Shoot;
    }
}