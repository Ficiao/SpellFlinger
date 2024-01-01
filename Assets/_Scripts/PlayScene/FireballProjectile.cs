using Fusion;
using System.Linq;
using UnityEngine;

namespace SpellFlinger.PlayScene
{
    public class FireballProjectile : Projectile
    {
        private bool _exploded = false;

        public override void Throw(Vector3 direction, PlayerRef ownerPlayerRef, PlayerStats ownerPlayerStats)
        {
            _direction = direction.normalized * _movementSpeed;
            _ownerPlayerRef = ownerPlayerRef;
            _ownerPlayerStats = ownerPlayerStats;
        }

        public override void Spawned()
        {
            if (Object.HasInputAuthority) Invoke("DelayEnable", 0.05f); 
        }

        private void DelayEnable() => enabled = true;


        public override void FixedUpdateNetwork()
        {
            _characterController.Move(_direction * Runner.DeltaTime);

            Collider[] hitColliders = Physics.OverlapSphere(transform.position, 0.6f);
            if (hitColliders.Any((collider) => collider.tag == "Ground")) Explode();

            foreach (Collider collider in hitColliders)
            {
                if (collider.tag == "Player")
                {
                    PlayerStats player = collider.GetComponent<PlayerStats>();
                    if (player.Object.StateAuthority != _ownerPlayerRef)
                    {
                        player.DealDamageRpc(_damage, _ownerPlayerStats);
                        if (!_exploded) Explode();
                    }
                }
            }
        }

        private void Explode()
        {
            _exploded = true;
            Debug.Log("Exploded");
            Destroy(gameObject);
        }
    }
}
