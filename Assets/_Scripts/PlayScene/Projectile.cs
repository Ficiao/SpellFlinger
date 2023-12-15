using Fusion;
using UnityEngine;

namespace SpellFlinger.PlayScene
{
    public abstract class Projectile : NetworkBehaviour
    {
        [SerializeField] protected float _movementSpeed;
        [SerializeField] protected CharacterController _characterController;
        protected Vector3 _direction;
        [Networked]
        public PlayerRef _ownerPlayer{get;set;}

        public abstract void Throw(Vector3 direction, PlayerRef ownerPlayer);
    }
}
