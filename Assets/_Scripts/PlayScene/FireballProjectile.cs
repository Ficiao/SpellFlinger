using Fusion;
using SpellFlinger.Enum;
using System.Linq;
using UnityEngine;

namespace SpellFlinger.PlayScene
{
    public class FireballProjectile : Projectile
    {
        [SerializeField] private float _range = 0f;
        [SerializeField] private float _explosionRange = 0f;
        [SerializeField] private float _explosionDuration = 0f;
        [SerializeField] private GameObject _projectileEffect = null;
        [SerializeField] private GameObject _explosionEffect = null;
        private bool _exploded = false;
        private PlayerStats _hitPlayer = null;

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
            if (_exploded) return;

            transform.Translate(_direction * Runner.DeltaTime);
            _effectModel.transform.rotation = Quaternion.FromToRotation(transform.forward, _direction.normalized);

            Collider[] hitColliders = Physics.OverlapSphere(transform.position, _range);

            foreach (Collider collider in hitColliders)
            {
                if (collider.tag != "Player") continue;

                PlayerStats player = collider.GetComponent<PlayerStats>();

                if (player.Object.StateAuthority == _ownerPlayerRef) continue;
                if (_ownerPlayerStats.Team != TeamType.None && player.Team == _ownerPlayerStats.Team) continue;

                player.DealDamageRpc(_damage, _ownerPlayerStats);
                _hitPlayer = player;
                Explode();

                break;
            }

            if (!_exploded && hitColliders.Any((collider) => collider.tag == "Ground")) Explode();
        }

        private void Explode()
        {
            _exploded = true;
            Debug.Log("Exploded");

            _projectileEffect.SetActive(false);
            _explosionEffect.SetActive(true);
            ExplodeEffectRpc();

            Collider[] hitColliders = Physics.OverlapSphere(transform.position, _explosionRange);

            foreach (Collider collider in hitColliders)
            {
                if (collider.tag != "Player") continue;

                PlayerStats player = collider.GetComponent<PlayerStats>();

                if (player.Object.StateAuthority == _ownerPlayerRef) continue;
                if (_ownerPlayerStats.Team != TeamType.None && player.Team == _ownerPlayerStats.Team) continue;
                if (player == _hitPlayer) continue;

                player.DealDamageRpc(_damage, _ownerPlayerStats);
            }

            Destroy(gameObject, _explosionDuration);
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void ExplodeEffectRpc()
        {
            if (HasStateAuthority) return;
            _projectileEffect.SetActive(false);
            _explosionEffect.SetActive(true);
        }

        //private void Update()
        //{
        //    Collider[] hitColliders = Physics.OverlapSphere(transform.position, _explosionRange);
        //    if (hitColliders.Any((collider) => collider.tag == "Ground")) Debug.Log("In range");
        //}
    }
}
