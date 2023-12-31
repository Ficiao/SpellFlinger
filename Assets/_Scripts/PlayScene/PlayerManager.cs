using SpellFlinger.Enum;
using System.Collections.Generic;
using UnityEngine;

namespace SpellFlinger.PlayScene
{
    public class PlayerManager : Singleton<PlayerManager>
    {
        [SerializeField] private Material _friendlyMaterial; 
        [SerializeField] private Material _enemyMaterial;        
        [SerializeField] private Color _friendlyColor; 
        [SerializeField] private Color _enemyColor; 
        private List<PlayerStats> _playerStats = new List<PlayerStats>();
        private TeamType _friendlyTeam = TeamType.None;

        public void RegisterPlayer(PlayerStats player)
        {
            _playerStats.Add(player);
            if(_friendlyTeam == TeamType.None || player.Team != _friendlyTeam) player.SetTeamMaterial(_enemyMaterial, _enemyColor);
            else player.SetTeamMaterial(_friendlyMaterial, _friendlyColor);
        }

        public void UnregisterPlayer(PlayerStats player) => _playerStats.Remove(player);

        public TeamType GetTeamWithLessPlayers()
        {
            int teamACount = 0;
            int teamBCount = 0;

            _playerStats.ForEach((player) =>
            {
                if (player.Team == TeamType.TeamA) teamACount++;
                else if (player.Team == TeamType.TeamB) teamBCount++;
            });

            return teamACount <= teamBCount ? TeamType.TeamA : TeamType.TeamB;
        }

        public void SetFriendlyTeam(TeamType friendlyTeam)
        {
            _friendlyTeam = friendlyTeam;

            _playerStats.ForEach((player) =>
            {
                if (player.Team == _friendlyTeam) player.SetTeamMaterial(_friendlyMaterial, _friendlyColor);
                else if (player.Team != TeamType.None) player.SetTeamMaterial(_enemyMaterial, _enemyColor);
            });

            UiManager.Instance.ShowTeamScore(friendlyTeam, _friendlyColor, _enemyColor);
        }
    }
}
