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

namespace SpellSlinger.Networking
{
    public class FusionConnection : SingletonPersistent<FusionConnection>, INetworkRunnerCallbacks
    {
        private static string _playerName = null;
        [SerializeField] private PlayerCharacterController _playerPrefab = null;
        [SerializeField] private NetworkRunner _networkRunnerPrefab = null;
        [SerializeField] private int _playerCount = 10;
        private NetworkRunner _runner = null;
        private NetworkSceneManagerDefault _networkSceneManager= null;
        private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();
        private List<SessionInfo> _sessions = new List<SessionInfo>();
        private GameModeType _gameModeType;
        public List<SessionInfo> Sessions => _sessions;

        public string PlayerName => _playerName;

        private void Awake()
        {
            base.Awake();
            _networkSceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>();
            _runner = gameObject.AddComponent<NetworkRunner>();
        }

        public void ConnectToLobby(String playerName = null)
        {
            if(!playerName.IsNullOrEmpty()) _playerName = playerName;
            _runner.JoinSessionLobby(SessionLobby.Shared);
        }

        public async void CreateSession(string sessionName, GameModeType gameMode, LevelType level)
        {
            _runner.ProvideInput = true;

            Dictionary<string, SessionProperty> properties = new Dictionary<string, SessionProperty>();
            properties.Add("level", SessionProperty.Convert((int)level));
            properties.Add("gameMode", SessionProperty.Convert((int)gameMode));
            _gameModeType = gameMode;

            await _runner.StartGame(new StartGameArgs()
            {
                GameMode = GameMode.Host,
                SessionName = sessionName,
                Scene = SceneRef.FromIndex((int)level),
                PlayerCount = _playerCount,
                SceneManager = _networkSceneManager,
                SessionProperties = properties,
            });
        }

        public async void JoinSession(string sessionName)
        {
            _runner.ProvideInput = true;

            await _runner.StartGame(new StartGameArgs()
            {
                GameMode = GameMode.Client,
                SessionName = sessionName,
            });
        }

        public void LeaveSession()
        {
            _runner.Shutdown();
            SceneManager.LoadScene(0);
        }

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        {
            if(_sessions == null || (_sessions.Count == 0 && sessionList.Count > 0))
            {
                _sessions = sessionList;
                SessionView.Instance.UpdateSessionList();
            }
            else _sessions = sessionList;
        }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            Debug.Log("On Player Joined");
            if (runner.IsServer)
            {
                NetworkObject playerObject = runner.Spawn(_playerPrefab.gameObject, inputAuthority: player);
                PlayerStats stats = playerObject.GetComponent<PlayerCharacterController>().PlayerStats;
                if (_gameModeType == GameModeType.TDM) stats.SetPlayerData(PlayerManager.Instance.GetTeamWithLessPlayers(), WeaponDataScriptable.SelectedWeaponType, _playerName);
                else
                {
                    stats.SetPlayerData(TeamType.None, WeaponDataScriptable.SelectedWeaponType, _playerName);
                    UiManager.Instance.ShowSoloScore();
                }
            }
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            Debug.Log("On Player Left");
            if (_spawnedCharacters.TryGetValue(player, out NetworkObject networkObject))
            {
                runner.Despawn(networkObject);
                _spawnedCharacters.Remove(player);
            }
        }

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            Debug.Log("On Input");

            var data = new NetworkInputData();

            if (CameraController.Instance &&!CameraController.Instance.CameraEnabled)
            {
                input.Set(data);
                return;
            }

            if (Input.GetKey(KeyCode.W))
                data.YDirection++;

            if (Input.GetKey(KeyCode.S))
                data.YDirection--;

            if (Input.GetKey(KeyCode.D))
                data.XDirection++;

            if (Input.GetKey(KeyCode.A))
                data.XDirection--;

            data.Jump = Input.GetKey(KeyCode.Space);

            data.YRotation = Input.GetAxis("Mouse X") * SensitivitySettingsScriptable.Instance.LeftRightSensitivity * runner.DeltaTime;
            data.Shoot = Input.GetMouseButton(0);

            input.Set(data);
        }

        private void OnApplicationQuit()
        {
            base.OnApplicationQuit();

            if (_runner != null && !_runner.IsDestroyed()) _runner.Shutdown();
        }

        public void OnConnectedToServer(NetworkRunner runner)
        {
            Debug.Log("On Connected to server");
        }

        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
        {
            Debug.Log("On Connect Failed");
        }

        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
        {
            Debug.Log("On Connect Request");
        }

        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
        {
            Debug.Log("On Custom Authentication Response");
        }

        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
        {
            Debug.Log("On Disconnected From Server");
        }

        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
        {
            Debug.Log("On Host Migration");
        }

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
        {
            Debug.Log("On Input Missing");
        }

        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
        {
            Debug.Log("On Object Enter AOI");
        }

        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
        {
            Debug.Log("OnO bject Exit AOI");
        }

        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
        {
            Debug.Log("On Reliable Data Progress");
        }

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
        {
            Debug.Log("On Reliable Data Received");
        }

        public void OnSceneLoadDone(NetworkRunner runner)
        {
            Debug.Log("On Scene Load Done");
        }

        public void OnSceneLoadStart(NetworkRunner runner)
        {
            Debug.Log("On Scene Load Start");
        }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            Debug.Log("On Shut down, reasone: " + shutdownReason.ToString());
        }

        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
        {
            Debug.Log("On User Simulation Message");
        }
    }
}