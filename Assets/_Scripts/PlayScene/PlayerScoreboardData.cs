using TMPro;
using UnityEngine;

namespace SpellFlinger.PlayScene
{
    public class PlayerScoreboardData : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _playerName = null;
        [SerializeField] private TextMeshProUGUI _kills = null;
        [SerializeField] private TextMeshProUGUI _deaths = null;

        public void Init(string playerName)
        {
            _playerName.text = playerName;
            _kills.text = "0";
            _deaths.text = "0";
        }

        public void UpdateScore(int kills, int deaths)
        {
            _kills.text = kills.ToString();
            _deaths.text = deaths.ToString();
        }
    }
}
