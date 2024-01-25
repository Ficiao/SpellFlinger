using Fusion;
using SpellFlinger.Enum;
using SpellFlinger.Scriptables;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace SpellFlinger.PlayScene
{
    public class PlayerStats : NetworkBehaviour
    {
        [SerializeField] private TextMeshPro _playerNameText = null;
        [SerializeField] private Slider _healthBar = null;
        [SerializeField] private int _maxHealth = 100;
        [SerializeField] private GameObject _playerModel = null;
        private bool _init = false;
        private PlayerScoreboardData _playerScoreboardData = null;
        private PlayerCharacterController _playerCharacterController = null;
        private float _slowDuation = 0f;
        public bool _isGameEnd = false;
        object _gameEndLock = new object();

        public bool IsSlowed => _slowDuation > 0.001f;

        [Networked] public NetworkString<_32> PlayerName { get; set; }
        [Networked] public TeamType Team { get; set; }
        private TeamType _oldTeamType = TeamType.None;
        [Networked] public WeaponType SelectedWeapon { get; set; }
        private WeaponType _oldWeapon;
        [Networked] public int Health { get; set; }
        private int _oldHealth = 0;
        [Networked] public int Kills { get; set; }
        private int _oldKills = 0;
        [Networked] public int Deaths { get; set; }
        private int _oldDeaths = 0;

        public override void Spawned()
        {
            _playerCharacterController = GetComponent<PlayerCharacterController>();
            PlayerManager.Instance.RegisterPlayer(this);
            if (HasStateAuthority) _playerNameText.gameObject.SetActive(false);
            //Adds Kills-1 because 1 will be added in custom change checker
            else if (Team != TeamType.None && Kills > 0) UiManager.Instance.AddTeamScore(Team, Kills - 1);
        }

        public void SetPlayerTeamAndWeapon(TeamType team, WeaponType weaponType)
        {
            Team = team;
            PlayerManager.Instance.SetFriendlyTeam(Team);
            SelectedWeapon = weaponType;
        }

        public void SetPlayerName(string playerName)
        {
            Health = _maxHealth;
            PlayerName = playerName;
            Init();
        }

        private void Init()
        {
            _playerNameText.text = PlayerName.ToString();
            _playerScoreboardData = UiManager.Instance.CreatePlayerScoarboardData();
            _playerScoreboardData.Init(PlayerName.ToString());
            Kills = 0;
            Deaths = 0;

            _init = true;
        }

        private void LateUpdate()
        {
            _playerNameText.transform.LookAt(CameraController.Instance.transform);
            _playerNameText.transform.Rotate(0, 180, 0);

            if (!_init && !string.IsNullOrEmpty(PlayerName.ToString())) Init();

            CustomChangeDetector();

            if (HasStateAuthority && _slowDuation > 0.001f) _slowDuation -= Time.deltaTime;
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void DealDamageRpc(int damage, PlayerStats attacker)
        {
            if (Health - damage <= 0)
            {
                if(Health > 0)
                {
                    Deaths++;
                    attacker.AddKillRpc();
                    _playerCharacterController.PlayerKilled();
                }
                Health = 0;
            }
            else Health -= damage;
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void ApplySlowRpc(float duration) => _slowDuation = duration;

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void AddKillRpc() => Kills++;

        public void ResetHealth() => Health = _maxHealth;

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void GameEndRpc(TeamType winnerTeam)
        {
            lock (_gameEndLock)
            {
                if (_isGameEnd) return;
                _isGameEnd = true;
            }

            Color winnerColor = Team == winnerTeam ? PlayerManager.Instance.FriendlyColor : PlayerManager.Instance.EnemyColor;
            _playerCharacterController.GameEnd(winnerTeam, winnerColor);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void GameEndRpc(string winnerName)
        {
            lock (_gameEndLock)
            {
                if (_isGameEnd) return;
                _isGameEnd = true;
            }

            Color winnerColor = PlayerName == winnerName ? PlayerManager.Instance.FriendlyColor : PlayerManager.Instance.EnemyColor;
            _playerCharacterController.GameEnd(winnerName, winnerColor);
        }

        private void CustomChangeDetector()
        {
            if (Health != _oldHealth)
            {
                _oldHealth = Health;
                if(!HasStateAuthority) _healthBar.value = (float)Health / _maxHealth;
                else UiManager.Instance.UpdateHealthBar(Health, (float)Health / _maxHealth);
            }

            if(Kills != _oldKills || Deaths != _oldDeaths)
            {
                if (Team == TeamType.None)
                {
                    if (HasStateAuthority)
                    {
                        UiManager.Instance.UpdateSoloScore(Kills);
                        if (Kills >= PlayerManager.Instance.SoloKillsForWin) PlayerManager.Instance.SendGameEndRpc(PlayerName.Value);
                    }
                }
                else if (Kills != _oldKills && Kills > 0)
                {
                    int teamKills = UiManager.Instance.IncreaseTeamScore(Team);
                    if(teamKills >= PlayerManager.Instance.TeamKillsForWin) PlayerManager.Instance.SendGameEndRpc(Team);
                }

                _playerScoreboardData.UpdateScore(Kills, Deaths);
                _oldKills = Kills;
                _oldDeaths = Deaths;
            }

            if(Team != _oldTeamType && Team != TeamType.None)
            {
                _oldTeamType = Team;
                _playerScoreboardData.SetTeamType(Team);
            }

            if(SelectedWeapon != _oldWeapon)
            {
                _oldWeapon = SelectedWeapon;
                var weaponData = WeaponDataScriptable.Instance.GetWeaponData(SelectedWeapon);
                _playerCharacterController.SetGloves(weaponData.GlovePrefab, weaponData.GloveLocation, weaponData.FireRate);
            }
        }

        public void SetTeamMaterial(Material material, Color color)
        {
            _playerNameText.color = color;
            if (!HasStateAuthority)
            {
                _playerModel.GetComponent<Renderer>().material = material;
            }
        }

        public void ResetGameInfo()
        {
            Kills = 0;
            Deaths = 0;
            UiManager.Instance.ResetScore();
            _isGameEnd = false;
        }

        private void OnDestroy()
        {
            if(_playerScoreboardData != null && !_playerScoreboardData.gameObject.IsDestroyed()) Destroy(_playerScoreboardData.gameObject);
            PlayerManager.Instance?.UnregisterPlayer(this);
        }
    }
}