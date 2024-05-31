using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System.Collections.Generic;
using System;
using SpellFlinger.PlayScene;
using SpellFlinger.Enum;
using SpellFlinger.LoginScene;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;
using SpellFlinger.Scriptables;
using WebSocketSharp;
using ExitGames.Client.Photon.StructWrapping;

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
        }

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {

            //var data = new NetworkInputData();

            //if (CameraController.Instance && !CameraController.Instance.CameraEnabled)
            //{
            //    input.Set(data);
            //    return;
            //}

            //if (Input.GetKey(KeyCode.W))
            //    data.YDirection++;

            //if (Input.GetKey(KeyCode.S))
            //    data.YDirection--;

            //if (Input.GetKey(KeyCode.D))
            //    data.XDirection++;

            //if (Input.GetKey(KeyCode.A))
            //    data.XDirection--;

            //data.buttons.Set(NetworkInputData.JUMP, Input.GetKey(KeyCode.Space));

            //data.buttons.Set(NetworkInputData.SHOOT, Input.GetMouseButton(0));

            //data.YRotation = _yRotation * runner.DeltaTime;
            //_yRotation = 0;


            //input.Set(data);
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