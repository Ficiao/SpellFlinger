using Fusion;
using SpellFlinger.Enum;
using SpellSlinger.Networking;
using System.Linq;
using UnityEngine;

namespace SpellFlinger.PlayScene
{
    public class IceSpikeProjectile : Projectile
    {
        [SerializeField] private float _range = 0f;
        [SerializeField] private float _dissolveDelay = 0f;
        [SerializeField] private float _slowDuration = 0f;
        [SerializeField] private GameObject _projectileModel = null;
        
        [Networked] public bool ProjectileHit { get; private set; }

        public override void Throw(Vector3 direction, PlayerStats ownerPlayerStats)
        {
            Direction = direction.normalized * _movementSpeed;
            OwnerPlayerStats = ownerPlayerStats;
            transform.rotation = Quaternion.FromToRotation(transform.forward, Direction.normalized);
        }

        public override void FixedUpdateNetwork()
        {
            if (ProjectileHit) return;

            /*
             * U ovoj metodi je potrebno napraviti detekciju pogotka, slično kao u metodi FixedNetworkUpdate
             * klase TeslaProjectile. Pri pogotku igrača uz štetu potrebno je pozvati metodu udaljene procedure
             * klase PlayerStats tog igrača koja će ga usporiti na odabrano vrijeme. 
             * U slučaju pogotka igrača potrebno je odmah uništiti objekt projektila, a ako je pogođen teren
             * projektil je potrebno uništiti nakon odgode vremena.
             */
            
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
        public void RPC_LockModelToPlayer(Vector3 localPosition, PlayerStats player)
        {
            _projectileModel.transform.parent = player.transform;
            _projectileModel.transform.localPosition = localPosition;
            Destroy(_projectileModel.gameObject, _dissolveDelay);
        }


        //This code can be used for testing hit range
        //private void Update()
        //{
        //    Collider[] hitColliders = Physics.OverlapSphere(transform.position, _range);
        //    if (hitColliders.Any((collider) => collider.tag == "Ground")) Debug.Log("In range");
        //}
    }    
}
