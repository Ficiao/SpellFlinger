using Fusion;
using SpellFlinger.Enum;
using SpellFlinger.Scriptables;
using SpellSlinger.Networking;
using System.Collections;
using UnityEngine;

namespace SpellFlinger.PlayScene
{
    public class PlayerCharacterController : NetworkBehaviour
    {
        [SerializeField] private Transform _cameraEndTarget = null;
        [SerializeField] private Transform _cameraStartTarget = null;
        [SerializeField] private Transform _cameraAimTarget = null;
        [SerializeField] private NetworkCharacterController _networkController;
        [SerializeField] private CharacterController _characterController;
        [SerializeField] private int _respawnTime = 0;
        [SerializeField] private Transform _shootOrigin;
        [SerializeField] private PlayerStats _playerStats = null;
        [SerializeField] private GameObject _playerModel = null;
        [SerializeField] private Transform _modelLeftHand = null;
        [SerializeField] private Transform _modelRightHand = null;
        [SerializeField] private Animator _playerAnimator = null;
        [SerializeField] private float _fireRate = 0;
        private PlayerAnimationController _playerAnimationController = null;
        private CameraController _cameraController = null;
        private Projectile _projectilePrefab = null;
        private PlayerAnimationState _playerAnimationState = PlayerAnimationState.Idle;
        private float _fireCooldown = 0;
        private IEnumerator _respawnCoroutine = null;

        public PlayerStats PlayerStats => _playerStats;
        public PlayerRef InputAuthority {  get; set; }

        public override void Spawned()
        {
            _playerAnimationController = new();
            if (HasInputAuthority) InitializeClient();
            if (Runner.IsServer) InitializeServer();
        }

        private void InitializeClient()
        {
            _cameraController = CameraController.Instance;
            _cameraController.transform.parent = _cameraEndTarget;
            _cameraController.Init(_cameraStartTarget, _cameraEndTarget);            
            _playerAnimationController.Init(ref _playerAnimationState, _playerAnimator);
            FusionConnection.Instance.LocalCharacterController = this;
        }

        private void InitializeServer()
        {
            _networkController.Teleport(SpawnLocationManager.Instance.GetRandomSpawnLocation());
            _playerAnimationController.Init(ref _playerAnimationState, _playerAnimator);
        }

        public void SetWeapon(WeaponDataScriptable.WeaponData data)
        {
            _projectilePrefab = data.WeaponPrefab;
            Instantiate(data.GlovePrefab, _modelLeftHand).transform.localPosition = data.GloveLocation;
            Instantiate(data.GlovePrefab, _modelRightHand).transform.localPosition = data.GloveLocation;
            _fireRate = data.FireRate;
        }

        public Vector3 GetShootDirection()
        {
            RaycastHit[] hits = Physics.RaycastAll(_cameraController.transform.position, _cameraController.transform.forward);
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.tag == "Projectile") continue;

                Vector3 shootDirection;
                if (Vector3.Dot(hit.point - _shootOrigin.position, transform.forward) >= 0)
                {
                    shootDirection = hit.point - _shootOrigin.position;
                    Debug.Log("Proper shoot");
                }
                else
                {
                    shootDirection = _cameraAimTarget.position - _shootOrigin.position;
                    Debug.Log("Bad shoot");
                }

                return shootDirection;
            }

            return _cameraAimTarget.position - _shootOrigin.position; ; 
        }

        private void Shoot(Vector3 shootDirection)
        {
            if (Time.time < _fireCooldown) return;

            _fireCooldown = Time.time + _fireRate;
            _playerAnimationController.PlayShootAnimation(_playerAnimator);

            Projectile projectile = Runner.Spawn(_projectilePrefab, _shootOrigin.position, inputAuthority: Runner.LocalPlayer);
            projectile.Throw(shootDirection, InputAuthority, _playerStats);
        }

        public override void FixedUpdateNetwork()
        {
            if (_playerStats.Health <= 0 || _characterController.enabled == false) return;

            if (GetInput(out NetworkInputData data))
            {
                if (HasStateAuthority && data.Buttons.IsSet(NetworkInputData.SHOOT)) Shoot(data.ShootTarget);

                _playerAnimationController.AnimationUpdate(_networkController.Grounded, data.Direction.x , data.Direction.y, ref _playerAnimationState, _playerAnimator, _playerModel.transform, transform);
                _networkController.Move(data.Direction, _playerStats.IsSlowed, data.Buttons.IsSet(NetworkInputData.JUMP), data.YRotation);             
            }
        }

        //This is called on server host, so HasStateAuthority is set to true
        public void PlayerKilled()
        {
            _respawnCoroutine = RespawnCoroutine();
            StartCoroutine(_respawnCoroutine);
        }

        //This is called on local player with InputAuthority
        [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority, HostMode = RpcHostMode.SourceIsServer)]
        public void RPC_PlayerKilled()
        {
            _respawnCoroutine = RespawnTimer();
            StartCoroutine(_respawnCoroutine);
        }

        private IEnumerator RespawnTimer()
        {
            _playerAnimationController.SetDeadState(ref _playerAnimationState, _playerAnimator);
            UiManager.Instance.ShowPlayerDeathScreen(_respawnTime);
            _cameraController.CameraEnabled = false;

            for (int i = 1; i < _respawnTime; i++)
            {
                yield return new WaitForSeconds(1);
                UiManager.Instance.UpdateDeathTimer(_respawnTime - i);
            }

            UiManager.Instance.HideDeathTimer();
            _playerAnimationController.SetAliveState(ref _playerAnimationState, _playerAnimator);
            _cameraController.CameraEnabled = true;
            _respawnCoroutine = null;
        }

        private IEnumerator RespawnCoroutine()
        {
            RPC_DisableController();

            yield return new WaitForSeconds(_respawnTime);

            _playerStats.ResetHealth();
            RPC_EnableController();
            _networkController.Teleport(SpawnLocationManager.Instance.GetRandomSpawnLocation());
            _characterController.enabled = true;
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void RPC_DisableController() => _characterController.enabled = false;

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void RPC_EnableController() => _characterController.enabled = true;

        public void GameEnd(TeamType winnerTeam, Color winnerColor)
        {
            UiManager.Instance.HideDeathTimer();
            UiManager.Instance.ShowEndGameScreen(winnerTeam, winnerColor);
            if (_respawnCoroutine != null)
            {
                StopCoroutine(_respawnCoroutine);
                _respawnCoroutine = null;
            }
            StartCoroutine(GameEndCountdown());
        }

        public void GameEnd(string winnerName, Color winnerColor)
        {
            UiManager.Instance.HideDeathTimer();
            UiManager.Instance.ShowEndGameScreen(winnerName, winnerColor);
            if (_respawnCoroutine != null)
            {
                StopCoroutine(_respawnCoroutine);
                _respawnCoroutine = null;
            }
            StartCoroutine(GameEndCountdown());
        }

        private IEnumerator GameEndCountdown()
        {
            _cameraController.CameraEnabled = false;
            _networkController.enabled = false;

            yield return new WaitForSeconds(7);

            UiManager.Instance.HideEndGameScreen();
            PlayerManager.Instance.ResetGameStats();
        }        
    }
}