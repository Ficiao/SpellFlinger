using Fusion;
using SpellFlinger.Enum;
using System.Linq;
using UnityEngine;

namespace SpellFlinger.PlayScene
{
    public class IceSpikeProjectile : Projectile
    {
        [SerializeField] private float _range = 0f;
        [SerializeField] private float _dissolveDelay = 0f;
        [SerializeField] private float _slowDuration = 0f;
        private bool _stopped = false;

        public override void Throw(Vector3 direction, PlayerRef ownerPlayerRef, PlayerStats ownerPlayerStats)
        {
            _direction = direction.normalized * _movementSpeed;
            _ownerPlayerRef = ownerPlayerRef;
            _ownerPlayerStats = ownerPlayerStats;
        }

        public override void FixedUpdateNetwork()
        {
            if (_stopped) return;

            /*
             * U ovoj metodi je potrebno napraviti detekciju pogotka, slično kao u metodi FixedNetworkUpdate
             * klase TeslaProjectile. Pri pogotku igrača uz štetu potrebno je pozvati metodu udaljene procedure
             * klase PlayerStats tog igrača koja će ga usporiti na odabrano vrijeme. 
             * U slučaju pogotka igrača potrebno je odmah uništiti objekt projektila, a ako je pogođen teren
             * projektil je potrebno uništiti nakon odgode vremena.
             */
        }

        //private void Update()
        //{
        //    Collider[] hitColliders = Physics.OverlapSphere(transform.position, _range);
        //    if (hitColliders.Any((collider) => collider.tag == "Ground")) Debug.Log("In range");
        //}
    }    
}
