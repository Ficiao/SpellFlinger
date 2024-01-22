using Fusion;
using UnityEngine;

namespace SpellFlinger.PlayScene
{
    public abstract class Projectile : NetworkBehaviour
    {
        [SerializeField] protected float _movementSpeed;
        [SerializeField] protected int _damage;
        [SerializeField] protected GameObject _effectModel;
        protected Vector3 _direction;

        [Networked] public PlayerStats _ownerPlayerStats {get;set;}
        [Networked] public PlayerRef _ownerPlayerRef {get;set;}

        public abstract void Throw(Vector3 direction, PlayerRef ownerPlayerRef, PlayerStats ownerPlayerStats);
    }
}
