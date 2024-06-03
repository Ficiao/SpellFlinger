using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System.Collections.Generic;
using System;
using SpellFlinger.PlayScene;
using SpellFlinger.Scriptables;

namespace SpellSlinger.Networking
{
    public class InputManager : SimulationBehaviour, IBeforeUpdate, INetworkRunnerCallbacks
    {
        private NetworkInputData _accumulatedInput;
        private bool _reset = false;

        public void BeforeUpdate()
        {
            if (_reset)
            {
                _reset = false;
                _accumulatedInput = default;
            }

            if (CameraController.Instance && !CameraController.Instance.CameraEnabled)
            {
                return;
            }

            Vector2 direction = Vector2.zero;
            NetworkButtons buttons = default;

            if (Input.GetKey(KeyCode.W))
                direction += Vector2.up;

            if (Input.GetKey(KeyCode.S))
                direction += Vector2.down;

            if (Input.GetKey(KeyCode.D))
                direction += Vector2.right;

            if (Input.GetKey(KeyCode.A))
                direction += Vector2.left;

            _accumulatedInput.Direction += direction;

            buttons.Set(NetworkInputData.JUMP, Input.GetKey(KeyCode.Space));

            buttons.Set(NetworkInputData.SHOOT, Input.GetMouseButton(0));
            if (Input.GetMouseButton(0)) _accumulatedInput.ShootTarget = FusionConnection.Instance.LocalCharacterController.GetShootDirection();

            _accumulatedInput.Buttons = new NetworkButtons(_accumulatedInput.Buttons.Bits | buttons.Bits);

            _accumulatedInput.YRotation += Input.GetAxis("Mouse X") * SensitivitySettingsScriptable.Instance.LeftRightSensitivity;
        }

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            _accumulatedInput.Direction.Normalize();
            input.Set(_accumulatedInput);

            _reset = true;
            _accumulatedInput.YRotation = 0f;
        }

        public void OnConnectedToServer(NetworkRunner runner) { }

        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }

        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }

        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }

        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }

        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }

        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }

        public void OnSceneLoadDone(NetworkRunner runner) { }

        public void OnSceneLoadStart(NetworkRunner runner) { }

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }

        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    }
}