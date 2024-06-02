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
        [SerializeField] private Transform _cameraEndTarget = null;
        [SerializeField] private Transform _cameraStartTarget = null;
        [SerializeField] private Transform _cameraAimTarget = null;
        [SerializeField] private NetworkCharacterController _networkController;
        [SerializeField] private int _respawnTime = 0;
        [SerializeField] private Transform _shootOrigin;
        [SerializeField] private PlayerStats _playerStats = null;
        [SerializeField] private GameObject _playerModel = null;
        [SerializeField] private Transform _modelLeftHand = null;
        [SerializeField] private Transform _modelRightHand = null;
        [SerializeField] private Animator _playerAnimator = null;
        private CameraController _cameraController = null;
        private Projectile _projectilePrefab = null;
        private PlayerAnimationState _playerAnimationState = PlayerAnimationState.Idle;
        [SerializeField] private float _fireRate = 0;
        private float _fireCooldown = 0;
        private bool _respawnReady = false;
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
            _networkController.Teleport(SpawnLocationManager.Instance.GetRandomSpawnLocation());
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

            if (_playerStats.Health <= 0 || _networkController.enabled == false) return;

            if (GetInput(out NetworkInputData data))
            {
                if (HasStateAuthority && data.Buttons.IsSet(NetworkInputData.SHOOT)) Shoot();

                _networkController.Move(data.Direction, _playerStats.IsSlowed, data.Buttons.IsSet(NetworkInputData.JUMP), data.YRotation);          
                
                PlayerAnimationController.AnimationUpdate(_networkController.Grounded, data.Direction.x , data.Direction.y, ref _playerAnimationState, _playerAnimator, _playerModel.transform, transform);
            }
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