using Fusion;
using JetBrains.Annotations;
using SpellFlinger.Enum;
using SpellFlinger.Scriptables;
using SpellSlinger.Networking;
using System;
using System.Collections.Generic;
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

        public Action OnSpawnedCallback;

        public bool IsSlowed => SlowDuration > 0.001f;

        [Networked, OnChangedRender(nameof(PlayerNameChanged))] public NetworkString<_32> PlayerName { get; set; }
        [Networked, OnChangedRender(nameof(TeamChanged))] public TeamType Team { get; set; }
        [Networked, OnChangedRender(nameof(WeaponChanged))] public WeaponType SelectedWeapon { get; set; }
        [Networked, OnChangedRender(nameof(HealthChanged))] public int Health { get; set; }
        [Networked, OnChangedRender(nameof(KillsChanged))] public int Kills { get; set; }
        [Networked, OnChangedRender(nameof(DeathsChanged))] public int Deaths { get; set; }
        [Networked] public float SlowDuration { get; set; }

        private void LateUpdate()
        {
            _playerNameText.transform.LookAt(CameraController.Instance.transform);
            _playerNameText.transform.Rotate(0, 180, 0);
        }

        public override void FixedUpdateNetwork()
        {
            if (HasStateAuthority && SlowDuration > 0.001f) SlowDuration -= Runner.DeltaTime;
        }

        public override void Spawned()
        {
            _playerCharacterController = GetComponent<PlayerCharacterController>();
            _playerScoreboardData = UiManager.Instance.CreatePlayerScoarboardData();
            PlayerManager.Instance.RegisterPlayer(this);
            if (HasInputAuthority)
            {
                _playerNameText.gameObject.SetActive(false);
                RPC_InitializeData(FusionConnection.Instance.PlayerName, WeaponDataScriptable.SelectedWeaponType);
            }

            if(PlayerName.Value != default) PlayerNameChanged();
            if (Team != default) TeamChanged();
            if (SelectedWeapon != default) WeaponChanged();
            if(Health != default) HealthChanged();
            if(Kills != default) KillsChanged();
            if(Deaths != default) DeathsChanged();
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
        public void RPC_InitializeData(string playerName, WeaponType selectedWeapon, RpcInfo info = default)
        {
            Kills = 0;
            Deaths = 0;
            Health = _maxHealth;
            PlayerName = playerName;
            SelectedWeapon = selectedWeapon;

            if(FusionConnection.GameModeType == GameModeType.TDM)
            {
                foreach (TeamType teamType in (TeamType[])System.Enum.GetValues(typeof(TeamType)))
                {
                    RPC_SetInitialTeamKills(teamType, PlayerManager.Instance.GetTeamKills(teamType));
                }
            }
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority, HostMode =RpcHostMode.SourceIsServer)]
        public void RPC_SetInitialTeamKills(TeamType team, int kills) => UiManager.Instance.SetTeamScore(team, kills);

        private void PlayerNameChanged()
        {
            _playerNameText.text = PlayerName.ToString();
            _playerScoreboardData.Init(PlayerName.ToString());
        }

        private void TeamChanged()
        {
            if (HasInputAuthority) PlayerManager.Instance.SetFriendlyTeam(Team);
            else PlayerManager.Instance.SetPlayerColor(this);
            _playerScoreboardData.SetTeamType(Team);
        }

        private void WeaponChanged()
        {
            var weaponData = WeaponDataScriptable.Instance.GetWeaponData(SelectedWeapon);
            _playerCharacterController.SetWeapon(weaponData);
        }

        private void HealthChanged()
        {
            if (!HasStateAuthority) _healthBar.value = (float)Health / _maxHealth;
            else UiManager.Instance.UpdateHealthBar(Health, (float)Health / _maxHealth);
        }

        private void KillsChanged()
        {
            if (FusionConnection.GameModeType == GameModeType.DM && HasInputAuthority) UiManager.Instance.UpdateSoloScore(Kills);
            else if (FusionConnection.GameModeType == GameModeType.TDM) UiManager.Instance.IncreaseTeamScore(Team);

            _playerScoreboardData.UpdateScore(Kills, Deaths);

            if (!HasStateAuthority) return;
        }

        private void DeathsChanged() => _playerScoreboardData.UpdateScore(Kills, Deaths);

        [Rpc(RpcSources.All, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
        public void DealDamageRpc(int damage, PlayerStats attacker)
        {
            if (Health - damage <= 0)
            {
                if(Health > 0)
                {
                    Deaths++;
                    attacker.Kills++;
                    _playerCharacterController.PlayerKilled();

                    if (FusionConnection.GameModeType == GameModeType.DM && Kills >= PlayerManager.Instance.SoloKillsForWin)
                    {
                        PlayerManager.Instance.SendGameEndRpc(PlayerName.Value);
                    }
                    else if (FusionConnection.GameModeType == GameModeType.TDM)
                    {
                        PlayerManager.Instance.AddTeamKill(Team);
                        if(PlayerManager.Instance.GetTeamKills(Team) >= PlayerManager.Instance.TeamKillsForWin) 
                        PlayerManager.Instance.SendGameEndRpc(Team);
                    }
                }
                Health = 0;
            }
            else Health -= damage;
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
        public void ApplySlowRpc(float duration) => SlowDuration = duration;

        public void ResetHealth() => Health = _maxHealth;

        [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority, HostMode = RpcHostMode.SourceIsServer)]
        public void GameEndRpc(TeamType winnerTeam)
        {
            Color winnerColor = Team == winnerTeam ? PlayerManager.Instance.FriendlyColor : PlayerManager.Instance.EnemyColor;
            _playerCharacterController.GameEnd(winnerTeam, winnerColor);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority, HostMode = RpcHostMode.SourceIsServer)]
        public void GameEndRpc(string winnerName)
        {
            Color winnerColor = PlayerName == winnerName ? PlayerManager.Instance.FriendlyColor : PlayerManager.Instance.EnemyColor;
            _playerCharacterController.GameEnd(winnerName, winnerColor);

        }

        public void SetTeamMaterial(Material material, Color color)
        {
            _playerNameText.color = color;
            if (!HasInputAuthority)
            {
                _playerModel.GetComponent<Renderer>().material = material;
            }
        }

        public void ResetGameInfo()
        {
            Kills = 0;
            Deaths = 0;
            UiManager.Instance.ResetScore();
        }

        private void OnDestroy()
        {
            if(_playerScoreboardData != null && !_playerScoreboardData.gameObject.IsDestroyed()) Destroy(_playerScoreboardData.gameObject);
            PlayerManager.Instance?.UnregisterPlayer(this);
        }
    }
}