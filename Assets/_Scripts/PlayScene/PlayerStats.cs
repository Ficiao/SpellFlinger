using Fusion;
using SpellFlinger.Enum;
using SpellFlinger.Scriptables;
using SpellSlinger.Networking;
using System;
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

        public PlayerCharacterController PlayerCharacterController => _playerCharacterController;
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
                if (FusionConnection.GameModeType == GameModeType.DM) UiManager.Instance.ShowSoloScore();
                else if (FusionConnection.GameModeType == GameModeType.TDM) UiManager.Instance.ShowTeamScore();
            }

            if(PlayerName.Value != default) PlayerNameChanged();
            if (Team != default) TeamChanged();
            if (SelectedWeapon != default) WeaponChanged();
            if(Health != default) HealthChanged();
            if(Kills != default) KillsChanged();
            if(Deaths != default) DeathsChanged();

            if(!HasInputAuthority && FusionConnection.GameModeType == GameModeType.DM) PlayerManager.Instance.SetPlayerColor(this);
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
        public void RPC_InitializeData(string playerName, WeaponType selectedWeapon, RpcInfo info = default)
        {
            Kills = 0;
            Deaths = 0;
            Health = _maxHealth;
            PlayerName = playerName;
            SelectedWeapon = selectedWeapon;
        }

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
            if (!HasInputAuthority) _healthBar.value = (float)Health / _maxHealth;
            else UiManager.Instance.UpdateHealthBar(Health, (float)Health / _maxHealth);
        }

        private void KillsChanged()
        {
            if (FusionConnection.GameModeType == GameModeType.DM && HasInputAuthority) UiManager.Instance.UpdateSoloScore(Kills);
            else if (FusionConnection.GameModeType == GameModeType.TDM) UiManager.Instance.UpdateTeamScore();

            _playerScoreboardData.UpdateScore(Kills, Deaths);
        }

        private void DeathsChanged() => _playerScoreboardData.UpdateScore(Kills, Deaths);

        public void DealDamage(int damage, PlayerStats attacker)
        {
            /*
             * U ovoj metodi je potrebno smanjiti živote igrača za štetu.
             * U slučaju da su životni bodovi prije poziva bili veći od 0, 
             * a nakon smanjivanja padnu na nula, potrebno je povećati broj
             * deathova igrača, obavijestiti PlayerCharacterController o smrti igrača,
             * te povećati broj killova igrača koji je napravio štetu pozivom udaljene procedure.
             */

            if (Health - damage <= 0)
            {
                if (Health == 0) return;

                Deaths++;
                attacker.Kills++;
                _playerCharacterController.PlayerKilled();

                if (FusionConnection.GameModeType == GameModeType.DM && attacker.Kills >= GameManager.Instance.SoloKillsForWin)
                {
                    GameManager.Instance.GameEnd(attacker.PlayerName.Value);
                }
                else if (FusionConnection.GameModeType == GameModeType.TDM)
                {
                    GameManager.Instance.AddTeamKill(attacker.Team);
                    if (GameManager.Instance.GetTeamKills(attacker.Team) >= GameManager.Instance.TeamKillsForWin)
                        GameManager.Instance.GameEnd(attacker.Team);
                }

                Health = 0;
            }
            else Health -= damage;
        }

        public void Heal(int healAmount)
        {
            Health += healAmount;
            if(Health > _maxHealth) Health  = _maxHealth;
        }

        public void ApplySlow(float duration) => SlowDuration = duration;

        public void ResetHealth() => Health = _maxHealth;

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
            Health = _maxHealth;
        }

        private void OnDestroy()
        {
            if(_playerScoreboardData != null && !_playerScoreboardData.gameObject.IsDestroyed()) Destroy(_playerScoreboardData.gameObject);
            PlayerManager.Instance?.UnregisterPlayer(this);
        }
    }
}