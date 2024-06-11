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
        [SerializeField] private float _doubleJumpDelay = 0f;
        private float _fireRate = 0;
        private PlayerAnimationController _playerAnimationController = null;
        private CameraController _cameraController = null;
        private Projectile _projectilePrefab = null;
        private PlayerAnimationState _playerAnimationState = PlayerAnimationState.Idle;
        private float _fireCooldown = 0;
        private IEnumerator _respawnCoroutine = null;
        private bool _resetPosition = false;
        private float _jumpTime = 0f;
        private bool _doubleJumpAvailable = false;
        private int _updatesSinceLastGrounded = 0;

        [Networked, OnChangedRender(nameof(RespawnTimeChanged))] public int RemainingRespawnTime { get; private set; }
        public PlayerStats PlayerStats => _playerStats;

        public override void Spawned()
        {
            _playerAnimationController = new();
            if (HasInputAuthority) InitializeClient();
            if (Runner.IsServer) InitializeServer();
        }

        private void InitializeClient()
        {
            FusionConnection.Instance.LocalCharacterController = this;
            _cameraController = CameraController.Instance;
            _cameraController.transform.parent = _cameraEndTarget;
            _cameraController.Init(_cameraStartTarget, _cameraEndTarget);            
            _playerAnimationController.Init(ref _playerAnimationState, _playerAnimator);
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

            if (HasStateAuthority)
            {
                Projectile projectile = Runner.Spawn(_projectilePrefab, _shootOrigin.position, inputAuthority: Runner.LocalPlayer);
                projectile.Throw(shootDirection, _playerStats);
            }
        }

        public override void FixedUpdateNetwork()
        {
            if(_resetPosition)
            {
                _networkController.enabled = true;
                _networkController.Teleport(SpawnLocationManager.Instance.GetRandomSpawnLocation());
                _resetPosition = false;
            }

            if (_playerStats.Health <= 0 || _characterController.enabled == false) return;

            if (GetInput(out NetworkInputData data))
            {
                if (data.Buttons.IsSet(NetworkInputData.SHOOT)) Shoot(data.ShootTarget);

                _playerAnimationController.AnimationUpdate(_networkController.Grounded, data.Direction.x , data.Direction.y, ref _playerAnimationState, _playerAnimator, _playerModel.transform, transform);

                bool isGrounded = _characterController.isGrounded;
                if (isGrounded) _updatesSinceLastGrounded = 0;
                else if (_updatesSinceLastGrounded < 3)
                {
                    isGrounded = true;
                    _updatesSinceLastGrounded++;
                }

                bool jump = data.Buttons.IsSet(NetworkInputData.JUMP);

                if (isGrounded && jump)
                {
                    _networkController.Jump();
                    _jumpTime = Time.time;
                }

                if (isGrounded) _doubleJumpAvailable = true;
                else if (jump && _doubleJumpAvailable && Time.time - _jumpTime >= _doubleJumpDelay)
                {
                    _doubleJumpAvailable = false;
                    _networkController.Jump(ignoreGrounded: true, doubleJump: true);
                    Debug.Log("Double jumped");
                }

                _networkController.Move(data.Direction, _playerStats.IsSlowed, data.YRotation, isGrounded);             
            }
        }

        //This is called on server host, so HasStateAuthority is set to true
        public void PlayerKilled()
        {
            RemainingRespawnTime = _respawnTime;
            _respawnCoroutine = RespawnCoroutine();
            StartCoroutine(_respawnCoroutine);
            RPC_PlayerKilled();
        }

        private IEnumerator RespawnCoroutine()
        {
            RPC_DisableController();
            _playerAnimationController.SetDeadState(ref _playerAnimationState, _playerAnimator);

            for(int i = RemainingRespawnTime; i > 0; i--)
            {
                yield return new WaitForSeconds(1);
                RemainingRespawnTime--;
            }

            _playerStats.ResetHealth();
            RPC_EnableController();
            _resetPosition = true;
            _playerAnimationController.SetAliveState(ref _playerAnimationState, _playerAnimator);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority, HostMode = RpcHostMode.SourceIsServer)]
        private void RPC_PlayerKilled()
        {
            _playerAnimationController.SetDeadState(ref _playerAnimationState, _playerAnimator);
            UiManager.Instance.ShowPlayerDeathScreen(_respawnTime);
            _cameraController.CameraLock = true;
            _cameraController.CameraEnabled = false;
        }

        private void RespawnTimeChanged()
        {
            if (!HasInputAuthority) return;

            UiManager.Instance.UpdateDeathTimer(RemainingRespawnTime);

            if (RemainingRespawnTime > 0) return;

            UiManager.Instance.HideDeathTimer();
            _playerAnimationController.SetAliveState(ref _playerAnimationState, _playerAnimator);
            if (GameManager.Instance.RemainingGameEndTime > 0) return;
            _cameraController.CameraLock = false;
            _cameraController.CameraEnabled = true;
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
        public void RPC_DisableController()
        {
            _networkController.enabled = false;
            _characterController.enabled = false;
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
        public void RPC_EnableController()
        {
            _networkController.enabled = true;
            _characterController.enabled = true;
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority, HostMode = RpcHostMode.SourceIsServer)]
        public void RPC_GameEnd() => GameEnd();

        public void GameEnd()
        {
            UiManager.Instance.HideDeathTimer();
            UiManager.Instance.ShowEndGameScreen();
            _cameraController.CameraLock = true;
            _cameraController.CameraEnabled = false;
        }
        
        public void StopRespawnCoroutine()
        {
            if (_respawnCoroutine == null) return;
            
            StopCoroutine(_respawnCoroutine);
            _respawnCoroutine = null;
            RemainingRespawnTime = 0;
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority, HostMode = RpcHostMode.SourceIsServer)]
        public void RPC_GameStart()
        {
            UiManager.Instance.HideEndGameScreen();
            _playerAnimationController.SetAliveState(ref _playerAnimationState, _playerAnimator);
            _cameraController.CameraLock = false;
            _cameraController.CameraEnabled = true;
        }

        public void SetGameStartPosition()
        {
            _resetPosition = true;
            _playerAnimationController.SetAliveState(ref _playerAnimationState, _playerAnimator);
        }
    }
}