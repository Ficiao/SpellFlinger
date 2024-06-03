using Fusion;
using SpellFlinger.Enum;
using SpellSlinger.Networking;
using System.Linq;
using UnityEngine;

namespace SpellFlinger.PlayScene
{
    public class TeslaProjectile : Projectile
    {
        [SerializeField] private float _range = 0f;

        public override void Throw(Vector3 direction, PlayerRef ownerPlayerRef, PlayerStats ownerPlayerStats)
        {
            _direction = direction.normalized * _movementSpeed;
            _ownerPlayerRef = ownerPlayerRef;
            _ownerPlayerStats = ownerPlayerStats;
        }

        public override void FixedUpdateNetwork()
        {
            transform.Translate(_direction * Runner.DeltaTime);
            _effectModel.transform.rotation = Quaternion.FromToRotation(transform.forward, _direction.normalized);

            Collider[] hitColliders = Physics.OverlapSphere(transform.position, _range);

            foreach (Collider collider in hitColliders)
            {
                if (collider.tag != "Player") continue;

                PlayerStats player = collider.GetComponent<PlayerStats>();

                if (player.Object.InputAuthority == _ownerPlayerRef) continue;
                if (FusionConnection.GameModeType == GameModeType.TDM && player.Team == _ownerPlayerStats.Team) continue;

                player.DealDamageRpc(_damage, _ownerPlayerStats);
                Destroy(gameObject);

                return;
            }

            if (hitColliders.Any((collider) => collider.tag == "Ground")) Destroy(gameObject);
        }

        //private void Update()
        //{
        //    Collider[] hitColliders = Physics.OverlapSphere(transform.position, _range);
        //    if (hitColliders.Any((collider) => collider.tag == "Ground")) Debug.Log("In range");
        //}
    }    
}
