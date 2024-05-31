using Fusion;
using SpellFlinger.Enum;
using SpellFlinger.Scriptables;
using System;
using System.Collections;
using UnityEngine;

namespace SpellFlinger.PlayScene
{
    public class PlayerCharacterController : NetworkBehaviour
    {
        [SerializeField] private float _angularSpeed = 0;
        [SerializeField] private Transform _cameraEndTarget = null;
        [SerializeField] private Transform _cameraStartTarget = null;
        [SerializeField] private Transform _cameraAimTarget = null;
        [SerializeField] private float _gravityBurst = 0;
        [SerializeField] private NetworkCharacterController _networkController;
        [SerializeField] private CharacterController _characterController;
        [SerializeField] private float _gravity = -9.81f;
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private float _jumpSpeed = 5f;
        [SerializeField] private int _respawnTime = 0;
        [SerializeField] private float _slopeRaycastDistance = 0f;
        [SerializeField] private Transform _shootOrigin;
        [SerializeField] private PlayerStats _playerStats = null;
        [SerializeField] private GameObject _playerModel = null;
        [SerializeField] private Transform _modelLeftHand = null;
        [SerializeField] private Transform _modelRightHand = null;
        [SerializeField] private Animator _playerAnimator = null;
        [SerializeField] private float _slowAmount = 0f;
        [SerializeField] private float _doubleJumpDelay = 0f;
        [SerializeField] private float _doubleJumpBoost = 1f;
        private float yVelocity = 0;
        private float _yRotation = 0;
        private CameraController _cameraController = null;
        private Projectile _projectilePrefab = null;
        private PlayerAnimationState _playerAnimationState = PlayerAnimationState.Idle;
        private int _updatesSinceLastGrounded = 0;
        [SerializeField] private float _fireRate = 0;
        private float _fireCooldown = 0;
        private bool _respawnReady = false;
        private bool _doubleJumpAvailable = false;
        private float _jumpTime = 0;
        private IEnumerator _respawnCoroutine = null;

        public PlayerStats PlayerStats => _playerStats;

        public override void Spawned()
        {
            if (HasInputAuthority) InitializeClient();
            if (Runner.IsServer) InitializeServer();
        }

        private void InitializeClient()
        {
            _cameraController = CameraController.Instance;
            _cameraController.transform.parent = _cameraEndTarget;
            _cameraController.Init(_cameraStartTarget, _cameraEndTarget);            
            PlayerAnimationController.Init(ref _playerAnimationState, _playerAnimator);
        }

        private void InitializeServer()
        {
            _characterController.enabled = false;
            transform.position = SpawnLocationManager.Instance.GetRandomSpawnLocation();
            _characterController.enabled = true;
            _projectilePrefab = WeaponDataScriptable.Instance.GetWeaponData(WeaponDataScriptable.SelectedWeaponType).WeaponPrefab;
            PlayerAnimationController.Init(ref _playerAnimationState, _playerAnimator);
        }

        public void SetGloves(GameObject glovesPrefab, Vector3 position, float fireRate)
        {
            Instantiate(glovesPrefab, _modelLeftHand).transform.localPosition = position;
            Instantiate(glovesPrefab, _modelRightHand).transform.localPosition = position;
            _fireRate = fireRate;
        }

        private void Shoot()
        {
            if (Time.time < _fireCooldown) return;

            _fireCooldown = Time.time + _fireRate;
            PlayerAnimationController.PlayShootAnimation(_playerAnimator);

            Projectile projectile = Runner.Spawn(_projectilePrefab, _shootOrigin.position, inputAuthority: Runner.LocalPlayer);
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

                projectile.Throw(shootDirection, Runner.LocalPlayer, _playerStats);
                break;
            }
        }

        public override void FixedUpdateNetwork()
        {
            if(_respawnReady) Respawn();

            bool isGrounded = _networkController.Grounded;
            if (isGrounded) _updatesSinceLastGrounded = 0;
            else if (_updatesSinceLastGrounded < 2) 
            {
                isGrounded = true;
                _updatesSinceLastGrounded++;
            }

            if (_playerStats.Health <= 0 || _networkController.enabled == false) return;

            if (GetInput(out NetworkInputData data))
            {
                if (Input.GetMouseButton(0)) Shoot();
                PlayerAnimationController.AnimationUpdate(isGrounded, data.XDirection, data.YDirection, ref _playerAnimationState, _playerAnimator, _playerModel.transform, transform);
                _yRotation += data.YRotation;
                Move(data.XDirection, data.YDirection, data.Jump, isGrounded);
            }
        }

        private void Move(int xDirection, int yDirection, bool jump, bool isGroundedFrameNormalized)
        {
            transform.eulerAngles = new Vector3(0f, _yRotation, 0f);

            Vector3 _moveDirection = transform.right * xDirection + transform.forward * yDirection;
            if (_playerStats.IsSlowed) _moveDirection *= _slowAmount;
            if (xDirection != 0 && yDirection != 0) _moveDirection /= (float)Math.Sqrt(2);

            if (_networkController.Grounded && jump)
            {
                _networkController.Jump();
            }

            if(isGroundedFrameNormalized) _doubleJumpAvailable = true;
            else if(jump && _doubleJumpAvailable  && Time.time - _jumpTime >= _doubleJumpDelay)
            {
                _doubleJumpAvailable = false;
                _networkController.Jump();
                Debug.Log("Double jumped");
            }

            _moveDirection = AdjustVelocityToSlope(_moveDirection);
            _networkController.Move(_moveDirection, _slopeRaycastDistance);
        }      

        private Vector3 AdjustVelocityToSlope(Vector3 velocity)
        {
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, _slopeRaycastDistance))
            {
                if (hit.collider.tag == "Ground")
                {
                    Quaternion slopeRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                    Vector3 adjustedVelocity = slopeRotation * velocity;

                    if (adjustedVelocity.y < 0) return adjustedVelocity;
                }
            }

            return velocity;
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void DisableControllerRpc()
        {
            if (HasStateAuthority) return;
            _networkController.enabled = false;
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void EnableControllerRpc()
        {
            if (HasStateAuthority) return;
            _networkController.enabled = true;
        }

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

            _respawnReady = true;
            UiManager.Instance.HideEndGameScreen();
            PlayerManager.Instance.ResetGameStats();
        }

        public void PlayerKilled()
        {
            PlayerAnimationController.SetDeadState(ref _playerAnimationState, _playerAnimator);
            _respawnCoroutine = RespawnCD();
            StartCoroutine(_respawnCoroutine);
        }

        private IEnumerator RespawnCD()
        {
            UiManager.Instance.ShowPlayerDeathScreen(_respawnTime);
            _cameraController.CameraEnabled = false;
            _networkController.enabled = false;
            DisableControllerRpc();

            for (int i = 1; i < _respawnTime; i++)
            {
                yield return new WaitForSeconds(1);
                UiManager.Instance.UpdateDeathTimer(_respawnTime - i);
            }

            _respawnReady = true;
            _respawnCoroutine = null;
        }

        private void Respawn()
        {
            _respawnReady = false;
            _playerStats.ResetHealth();
            UiManager.Instance.HideDeathTimer();
            PlayerAnimationController.SetAliveState(ref _playerAnimationState, _playerAnimator);
            transform.position = SpawnLocationManager.Instance.GetRandomSpawnLocation();
            EnableControllerRpc();
            _networkController.enabled = true;
            _cameraController.CameraEnabled = true;
        }
    }
}