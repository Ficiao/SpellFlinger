using Fusion;
using SpellSlinger.Networking;
using TMPro;
using UnityEngine;

namespace SpellFlinger.PlayScene
{
    public class PlayerStats : NetworkBehaviour
    {
        [SerializeField] private TextMeshPro _playerNameText = null;
        [SerializeField] private int _health;
        [SerializeField] private int _maxHealth = 100;
        [SerializeField] private int _itemAmount = 0;
        [SerializeField] private int _maxItemAmount = 3;
        private bool _init = false;

        [Networked] public NetworkString<_32> PlayerName { get; set; }
        public int Health => _health;

        public void SetPlayerName(string playerName)
        {
            PlayerName = playerName;
            _playerNameText.text = playerName;
            _init = true;
        }

        public void Init()
        {
            _health = _maxHealth;
            PlayerName = FusionConnection.Instance.PlayerName;
        }

        private void LateUpdate()
        {
            _playerNameText.transform.LookAt(CameraController.Instance.transform);
            _playerNameText.transform.Rotate(0, 180, 0);
            if (!_init)
            {
                if (!string.IsNullOrEmpty(PlayerName.ToString()))
                {
                    _playerNameText.text = PlayerName.ToString();
                    _init = true;
                }
            }
        }
    }
}