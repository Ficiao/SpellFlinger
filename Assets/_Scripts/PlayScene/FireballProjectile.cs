using Fusion;
using System.Collections.Generic;
using UnityEngine;

namespace SpellFlinger.PlayScene
{
    public class FireballProjectile : Projectile
    {
        public override void Throw(Vector3 direction, PlayerRef ownerPlayer)
        {
            _direction = direction.normalized * _movementSpeed;
            _ownerPlayer = ownerPlayer;
        }

        public override void Spawned()
        {
            if (Object.HasInputAuthority) Invoke("DelayEnable", 0.05f); 
        }

        private void DelayEnable() => enabled = true;


        public override void FixedUpdateNetwork()
        {
            if (HasStateAuthority) return;
            _characterController.Move(_direction * Runner.DeltaTime);

            Collider[] hitColliders = Physics.OverlapSphere(transform.position, 0.6f);
            foreach (Collider collider in hitColliders)
            {
                if (collider.tag == "Ground")
                {
                    Explode();
                    Destroy(gameObject);
                    return;
                }
            }
        }

        private void Explode()
        {
            Debug.Log("Exploded");
        }
    }
}
