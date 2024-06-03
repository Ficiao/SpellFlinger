using SpellFlinger.Enum;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace SpellFlinger.PlayScene
{
    public class PlayerManager : Singleton<PlayerManager>
    {
        [SerializeField] private Material _friendlyMaterial;
        [SerializeField] private Material _enemyMaterial;
        [SerializeField] private Color _friendlyColor;
        [SerializeField] private Color _enemyColor;
        [SerializeField] private int _teamKillsForWin = 0;
        [SerializeField] private int _soloKillsForWin = 0;
        private List<PlayerStats> _players = null;
        private TeamType _friendlyTeam = TeamType.None;
        private Dictionary<TeamType, int> _teamKills = null;

        public Action OnPlayerTeamTypeSet = null;
        public TeamType FriendlyTeam => _friendlyTeam;
        public Color FriendlyColor => _friendlyColor;
        public Color EnemyColor => _enemyColor;
        public int TeamKillsForWin => _teamKillsForWin;
        public int SoloKillsForWin => _soloKillsForWin;

        private void Awake()
        {
            base.Awake();
            _players = new();
            _teamKills = new();
            foreach(TeamType teamType in (TeamType[])System.Enum.GetValues(typeof(TeamType)))
            {
                _teamKills.Add(teamType, 0);
            }
        }

        public void RegisterPlayer(PlayerStats player) => _players.Add(player);

        public void UnregisterPlayer(PlayerStats player) => _players.Remove(player);

        public TeamType GetTeamWithLessPlayers()
        {
            int teamACount = 0;
            int teamBCount = 0;

            _players.ForEach((player) =>
            {
                if (player.Team == TeamType.TeamA) teamACount++;
                else if (player.Team == TeamType.TeamB) teamBCount++;
            });

            return teamACount <= teamBCount ? TeamType.TeamA : TeamType.TeamB;
        }

        public void SetFriendlyTeam(TeamType friendlyTeam)
        {
            _friendlyTeam = friendlyTeam;
            _players.ForEach((player) => SetPlayerColor(player));
            OnPlayerTeamTypeSet?.Invoke();
        }

        public void SetPlayerColor(PlayerStats player)
        {
            if (_friendlyTeam == TeamType.None || player.Team != _friendlyTeam) player.SetTeamMaterial(_enemyMaterial, _enemyColor);
            else player.SetTeamMaterial(_friendlyMaterial, _friendlyColor);
        }

        public void AddTeamKill(TeamType team) => _teamKills[team]++;
        
        public void SetTeamKills(TeamType team, int kill) => _teamKills[team] = kill;

        public int GetTeamKills(TeamType team) => _teamKills[team];

        public void ResetTeamKills()
        {
            foreach (TeamType team in _teamKills.Keys) _teamKills[team] = 0;
        }

        public void SendGameEndRpc(string winnerName) => _players.ForEach((player) => player.GameEndRpc(winnerName));

        public void SendGameEndRpc(TeamType winnerTeam) => _players.ForEach((player) => player.GameEndRpc(winnerTeam));

        public void ResetGameStats() => _players.ForEach((player) => player.ResetGameInfo());
    }
}
