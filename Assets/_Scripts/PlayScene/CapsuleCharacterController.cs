using Fusion;
using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace SpellFlinger.PlayScene
{
    public class CapsuleCharacterController : NetworkBehaviour
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
        [SerializeField] private Transform _shootOrigin;
        [SerializeField] private PlayerStats _playerStats = null;
        [SerializeField] private Projectile _projectilePrefab = null;
        private bool[] inputs;
        private float yVelocity = 0;
        private float _yRotation;
        private CameraController _cameraController;

        public String PlayerName
        {
            get => _playerStats.PlayerName.ToString();
            set => _playerStats.SetPlayerName(value);
        }

        public override void Spawned()
        {
            if (HasStateAuthority)
            {
                Initialize(1, new Vector3(0, 0, 0));
                _playerStats.Init();
            }
        }

        public void Initialize(int id, Vector3 spawnPosition)
        {
            _cameraController = CameraController.Instance;
            _cameraController.transform.parent = _cameraEndTarget;
            _cameraController.Init(_cameraStartTarget, _cameraEndTarget);
            inputs = new bool[5];
        }

        public void Update()
        {
            inputs[0]=Input.GetKey(KeyCode.W);
            inputs[1]=Input.GetKey(KeyCode.S);
            inputs[2]=Input.GetKey(KeyCode.A);
            inputs[3]=Input.GetKey(KeyCode.D);
            inputs[4]=Input.GetKey(KeyCode.Space);

            if (_cameraController.CameraEnabled) _yRotation += Input.GetAxis("Mouse X") * _angularSpeed * Runner.DeltaTime;

            if(Input.GetMouseButtonDown(0)) 
            {
                Projectile projectile = Runner.Spawn(_projectilePrefab, _shootOrigin.position, inputAuthority: Runner.LocalPlayer);
                RaycastHit hit;
                if (Physics.Raycast(_cameraController.ShootPoint.position, _cameraController.transform.forward, out hit))
                {
                    if(Vector3.Dot(hit.point - _shootOrigin.position, transform.forward) >= 0) projectile.Throw(hit.point - _shootOrigin.position, Runner.LocalPlayer);
                    else projectile.Throw(_cameraAimTarget.position - _shootOrigin.position, Runner.LocalPlayer);
                }
                else Debug.Log("Couldnt find target.");
            }
        }

        public override void FixedUpdateNetwork()
        {
            if (_playerStats.Health > 0)
            {
                Vector2 _inputDirection = Vector2.zero;
                if (inputs[0])
                {
                    _inputDirection.y += 1;
                }
                if (inputs[1])
                {
                    _inputDirection.y -= 1;
                }
                if (inputs[2])
                {
                    _inputDirection.x -= 1;
                }
                if (inputs[3])
                {
                    _inputDirection.x += 1;
                }

                Move(_inputDirection);
            }
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
    }
}