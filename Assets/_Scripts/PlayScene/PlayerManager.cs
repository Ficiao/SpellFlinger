using SpellFlinger.Enum;
using System;
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
        [SerializeField] private int _teamKillsForWin = 0;
        [SerializeField] private int _soloKillsForWin = 0;
        private List<PlayerStats> _playerStats = new List<PlayerStats>();
        private TeamType _friendlyTeam = TeamType.None;

        public Action OnPlayerTeamTypeSet;
        public TeamType FriendlyTeam => _friendlyTeam;
        public Color FriendlyColor => _friendlyColor;
        public Color EnemyColor => _enemyColor;
        public int TeamKillsForWin => _teamKillsForWin;
        public int SoloKillsForWin => _soloKillsForWin;

        public void RegisterPlayer(PlayerStats player)
        {
            /*
             * Potrebno je dodati igrača u listu registriranih, te mu postaviti timsku boju slično 
             * načinu kako se to obavlja u metodi SetFriendlyTeam.
             */
        }

        public void UnregisterPlayer(PlayerStats player) => _playerStats.Remove(player);

        public TeamType GetTeamWithLessPlayers()
        {
            /*
             * Potrebno je zamijeniti liniju return (TeamType)(-1); tako da metoda vraća
             * TeamType s timom koji ima manje igrača.
             */

            return (TeamType)(-1);
        }

        public void SetFriendlyTeam(TeamType friendlyTeam)
        {
            _friendlyTeam = friendlyTeam;

            _playerStats.ForEach((player) =>
            {
                if (player.Team == _friendlyTeam) player.SetTeamMaterial(_friendlyMaterial, _friendlyColor);
                else if (player.Team != TeamType.None) player.SetTeamMaterial(_enemyMaterial, _enemyColor);
            });

            OnPlayerTeamTypeSet.Invoke();
        }

        public void SendGameEndRpc(string winnerName)
        {
            _playerStats.ForEach((player) => player.GameEndRpc(winnerName));
        }

        public void SendGameEndRpc(TeamType winnerTeam)
        {
            _playerStats.ForEach((player) => player.GameEndRpc(winnerTeam));
        }

        public void ResetGameStats()
        {
            _playerStats.ForEach((player) => player.ResetGameInfo());  
        }
    }
}
