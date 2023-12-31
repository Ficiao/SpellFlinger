using Fusion;
using SpellFlinger.Enum;
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
        [SerializeField] private int _itemAmount = 0;
        [SerializeField] private int _maxItemAmount = 3;
        [SerializeField] private GameObject _playerModel = null;
        private bool _init = false;
        private PlayerScoreboardData _playerScoreboardData = null;
        private PlayerCharacterController _capsuleCharacterController = null;

        [Networked] public NetworkString<_32> PlayerName { get; set; }
        [Networked] public TeamType Team { get; set; }
        [Networked] public int Health { get; set; }
        private int _oldHealth = 0;
        [Networked] public int Kills { get; set; }
        private int _oldKills = 0;
        [Networked] public int Deaths { get; set; }
        private int _oldDeaths = 0;

        public override void Spawned()
        {
            _capsuleCharacterController = GetComponent<PlayerCharacterController>();
            PlayerManager.Instance.RegisterPlayer(this);
        }

        public void SetPlayerTeam(TeamType team)
        {
            Team = team;
            PlayerManager.Instance.SetFriendlyTeam(Team);
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
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void DealDamageRpc(int damage, PlayerStats attacker)
        {
            if (Team != TeamType.None && attacker.Team == Team) return;

            if (Health - damage <= 0)
            {
                if(Health > 0)
                {
                    Deaths++;
                    attacker.AddKillRpc();
                    _capsuleCharacterController.PlayerKilled();
                }
                Health = 0;
            }
            else Health -= damage;
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void AddKillRpc() => Kills++;

        public void ResetHealth() => Health = _maxHealth;

        private void CustomChangeDetector()
        {
            if (Health != _oldHealth)
            {
                _oldHealth = Health;
                _healthBar.value = (float)Health / _maxHealth;
            }

            if(Kills != _oldKills || Deaths != _oldDeaths)
            {
                if (Team == TeamType.None && HasStateAuthority) UiManager.Instance.UpdateSoloScore(Kills);
                else if (Kills != _oldKills) UiManager.Instance.IncreaseTeamScore(Team);
                else UiManager.Instance.IncreaseTeamScore(Team == TeamType.TeamA ? TeamType.TeamB : TeamType.TeamA);

                _playerScoreboardData.UpdateScore(Kills, Deaths);
                _oldKills = Kills;
                _oldDeaths = Deaths;
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

        private void OnDestroy()
        {
            if(_playerScoreboardData != null && !_playerScoreboardData.gameObject.IsDestroyed()) Destroy(_playerScoreboardData.gameObject);
            PlayerManager.Instance.UnregisterPlayer(this);
        }
    }
}