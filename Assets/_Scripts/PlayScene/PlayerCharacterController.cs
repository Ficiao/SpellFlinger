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
        [SerializeField] private CharacterController _controller;
        [SerializeField] private float _gravity = -9.81f;
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private float _jumpSpeed = 5f;
        [SerializeField] private float _thorwForce = 600f;
        [SerializeField] private int _respawnTime = 0;
        [SerializeField] private Transform _shootOrigin;
        [SerializeField] private PlayerStats _playerStats = null;
        [SerializeField] private GameObject _playerModel = null;
        [SerializeField] private Animator _playerAnimator = null;
        private bool[] inputs = null;
        private float yVelocity = 0;
        private float _yRotation = 0;
        private CameraController _cameraController = null;
        private Projectile _projectilePrefab = null;
        private PlayerAnimationState _playerAnimationState = PlayerAnimationState.Idle;
        private int _updatesSinceLastGrounded = 0;

        public PlayerStats PlayerStats => _playerStats;

        public override void Spawned()
        {
            if (HasStateAuthority) Initialize(1, new Vector3(0, 0, 0));
        }

        public void Initialize(int id, Vector3 spawnPosition)
        {
            _cameraController = CameraController.Instance;
            _cameraController.transform.parent = _cameraEndTarget;
            _cameraController.Init(_cameraStartTarget, _cameraEndTarget);
            _controller.enabled = false;
            transform.position = SpawnLocationManager.Instance.GetRandomSpawnLocation();
            _controller.enabled = true;
            _projectilePrefab = WeaponDataScriptable.Instance.GetWeaponData(WeaponDataScriptable.SelectedWeaponType).WeaponPrefab;
            inputs = new bool[5];
            PlayerAnimationController.Init(ref _playerAnimationState, _playerAnimator);
        }

        public void Update()
        {
            inputs[0]=Input.GetKey(KeyCode.W);
            inputs[1]=Input.GetKey(KeyCode.S);
            inputs[2]=Input.GetKey(KeyCode.A);
            inputs[3]=Input.GetKey(KeyCode.D);
            inputs[4]=Input.GetKey(KeyCode.Space);

            if (!_cameraController.CameraEnabled) return;

            _yRotation += Input.GetAxis("Mouse X") * _angularSpeed * Runner.DeltaTime;
            if (Input.GetMouseButtonDown(0)) Shoot();
        }

        private void Shoot()
        {
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
            bool isGrounded = _controller.isGrounded;
            if (isGrounded) _updatesSinceLastGrounded = 0;
            else if (_updatesSinceLastGrounded < 2) 
            {
                isGrounded = true;
                _updatesSinceLastGrounded++;
            }

            if (_playerStats.Health <= 0) return;

            Vector2 _inputDirection = Vector2.zero;
            if (inputs[0]) _inputDirection.y += 1;
            if (inputs[1]) _inputDirection.y -= 1;
            if (inputs[2]) _inputDirection.x -= 1;
            if (inputs[3]) _inputDirection.x += 1;

            PlayerAnimationController.AnimationUpdate(isGrounded, (int)_inputDirection.x, (int)_inputDirection.y, ref _playerAnimationState, _playerAnimator, _playerModel.transform, transform);
            Move(_inputDirection);
        }

        private void Move(Vector2 _inputDirection)
        {
            transform.eulerAngles = new Vector3(0f, _yRotation, 0f);

            Vector3 _moveDirection = transform.right * _inputDirection.x + transform.forward * _inputDirection.y;
            _moveDirection *= _moveSpeed * Runner.DeltaTime;
            if (_inputDirection.x != 0 && _inputDirection.y != 0) _moveDirection /= (float)Math.Sqrt(2);

            float deltaGravity = _gravity * Runner.DeltaTime;
            yVelocity += deltaGravity;
            if (Math.Abs(yVelocity) < deltaGravity)
            {
                yVelocity += _gravity * _gravityBurst;
            }
            if (_controller.isGrounded)
            {
                yVelocity = 0f;
                if (inputs[4])
                {
                    yVelocity = _jumpSpeed;
                }
            }

            _moveDirection.y = yVelocity;
            _controller.Move(_moveDirection);
        }      

        public void PlayerKilled()
        {
            PlayerAnimationController.SetDeadState(ref _playerAnimationState, _playerAnimator);
            StartCoroutine(Respawn());
        }

        private IEnumerator Respawn()
        {
            UiManager.Instance.ShowPlayerDeathScreen(_respawnTime);

            for(int i = 1; i < _respawnTime; i++)
            {
                yield return new WaitForSeconds(1);
                UiManager.Instance.UpdateDeathTimer(_respawnTime - i);
            }

            UiManager.Instance.HideDeathTimer();
            PlayerAnimationController.SetAliveState(ref _playerAnimationState, _playerAnimator);
            _playerStats.ResetHealth();
            _controller.enabled = false;
            transform.position = SpawnLocationManager.Instance.GetRandomSpawnLocation();
            _controller.enabled = true;
        }
    }
}