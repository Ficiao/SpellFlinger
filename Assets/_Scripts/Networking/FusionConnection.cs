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
        [SerializeField] private NetworkRunner _runner = null;
        [SerializeField] private NetworkSceneManagerDefault _networkSceneManager= null;
        private List<SessionInfo> _sessions = new List<SessionInfo>();
        private GameModeType _gameModeType;
        public List<SessionInfo> Sessions => _sessions;

        public string PlayerName => _playerName;

        private void Awake()
        {
            base.Awake();
            _networkSceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>();
        }

        public void ConnectToLobby(String playerName = null)
        {
            if(!_playerName.IsNullOrEmpty()) _playerName = playerName;
            _runner.JoinSessionLobby(SessionLobby.Shared);
        }

        public async void JoinSession(string sessionName, GameModeType gameMode, LevelType level)
        {

            _runner.ProvideInput = false;

            /* U ovoj metodi potrebno je lokalno cache-irati odabrani način igre, te pozvati metodu StartGame NetworkRunner instance koja igrača spaja u sobu.
             * StartGame metoda prima argument tipa strukture. Potrebno je napraviti novu instancu strukture, te joj inicijalizirati vrijednosti.
             * Potrebno je postaviti način igre na Shared, proslijediti ime sesije, scenu koja se treba učitat nakon spajanja u sobu (parametar se 
             * predaje u obliku SceneRef.FromIndex()), maksimalni broj igrača, scene manager iz lokalne reference i SessionProperties. 
             * U SessionProperties ulaze custom svojstva, u ovom slučaju su to tip igre i level koji se treba učitati. Proučite kojeg tipa je 
             * SessionPropeties, te mu proslijedite sva potrebna svojstva. (tip: za pretvaranje custom svojstva u pogodan oblik može se koristiti
             * SessionProperty.Convert() metoda.
             */
        }

        public void LeaveSession()
        {
            /*
             * U ovoj metodi je potrebno pozvati Shutdown metodu intance NetworkRunner klase, te učitati početni ekran.
             */
        }

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        {
            /*
             * U ovoj metodi je potrebno lokalno spremiti osvježenu listu soba, te osviježiti prikaz liste soba.
             */
        }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            Debug.Log("On Player Joined");
            if (player == runner.LocalPlayer)
            {
                NetworkObject playerObject = runner.Spawn(_playerPrefab.gameObject);
                PlayerCharacterController characterController = playerObject.GetComponent<PlayerCharacterController>();
                characterController.enabled = true;
                runner.SetPlayerObject(runner.LocalPlayer, playerObject);
                PlayerStats stats = characterController.PlayerStats;
                stats.SetPlayerName(_playerName);
                if (_gameModeType == GameModeType.TDM) stats.SetPlayerTeamAndWeapon(PlayerManager.Instance.GetTeamWithLessPlayers(), WeaponDataScriptable.SelectedWeaponType);
                else
                {
                    stats.SetPlayerTeamAndWeapon(TeamType.None, WeaponDataScriptable.SelectedWeaponType);
                    UiManager.Instance.ShowSoloScore();
                }
            }
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

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            Debug.Log("On Input");
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

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            Debug.Log("On Player Left");
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