using SpellFlinger.Enum;
using SpellSlinger.Networking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpellFlinger.PlayScene
{
    public class UiManager : Singleton<UiManager>
    {
        [SerializeField] private Button _pauseButton = null;
        [SerializeField] private GameObject _pauseMenu = null;
        [SerializeField] private Button _returnButton = null;
        [SerializeField] private Button _leaveGameButton = null;
        [SerializeField] private GameObject _scoreMenu = null;
        [SerializeField] private Transform _scoreBoardContainer = null;
        [SerializeField] private PlayerScoreboardData _scoreBoardDataPrefab = null;
        [SerializeField] private GameObject _deathScreen = null;
        [SerializeField] private TextMeshProUGUI _deathTimer = null;
        [SerializeField] private GameObject _aimCursor = null;
        [SerializeField] private GameObject _teamScore = null;
        [SerializeField] private GameObject _soloScore = null;
        [SerializeField] private TextMeshProUGUI _teamAScoreText = null;
        [SerializeField] private TextMeshProUGUI _teamBScoreText = null;
        [SerializeField] private TextMeshProUGUI _soloScoreText = null;
        private int _teamAKills = 0;
        private int _teamBKills = 0;

        private void Awake()
        {
            base.Awake();
            _pauseButton.onClick.AddListener(() => _pauseMenu.SetActive(true));
            _returnButton.onClick.AddListener(() =>
            {
                _pauseMenu.SetActive(false);
                CameraController.Instance.CameraEnabled = true;
            });
            _leaveGameButton.onClick.AddListener(() =>
            {
                CameraController.Instance.CameraEnabled = false;
                FusionConnection.Instance.LeaveSession();
            });
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.LeftAlt)) _pauseMenu.SetActive(false);
            if (Input.GetKeyDown(KeyCode.Tab)) _scoreMenu.SetActive(true);
            if (Input.GetKeyUp(KeyCode.Tab)) _scoreMenu.SetActive(false);
        }

        public PlayerScoreboardData CreatePlayerScoarboardData() => Instantiate(_scoreBoardDataPrefab, _scoreBoardContainer);

        public void ShowPlayerDeathScreen(int timer)
        {
            _deathScreen.SetActive(true);
            _deathTimer.text = timer.ToString();
            _aimCursor.SetActive(false);
        }

        public void UpdateDeathTimer(int time) => _deathTimer.text = time.ToString();

        public void HideDeathTimer()
        {
            _deathScreen.SetActive(false);
            _aimCursor.SetActive(true);
        }

        public void ShowTeamScore(TeamType friendlyTeam, Color friendlyColor, Color enemyColor)
        {
            _teamScore.SetActive(true);
            _teamAScoreText.text = "Team A: 0";
            _teamBScoreText.text = "Team B: 0";

            if (friendlyTeam == TeamType.TeamA)
            {
                _teamAScoreText.color = friendlyColor;
                _teamBScoreText.color = enemyColor;
            }
            else 
            {
                _teamAScoreText.color = enemyColor;
                _teamBScoreText.color = friendlyColor;
            }
        }

        public void ShowSoloScore()
        {
            _soloScore.SetActive(true); 
            _soloScoreText.text = "Kill: 0";
        }

        public void IncreaseTeamScore(TeamType team)
        {
            if (team == TeamType.TeamA)
            {
                _teamAKills++;
                _teamAScoreText.text = "Team A: " + _teamAKills.ToString();
            }
            else
            {
                _teamBKills++;
                _teamBScoreText.text = "Team B: " + _teamBKills.ToString();
            }
        }

        public void UpdateSoloScore(int kills) => _soloScoreText.text = "Kills: " + kills.ToString();
    }
}
