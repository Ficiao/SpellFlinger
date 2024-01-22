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
        [SerializeField] private TextMeshProUGUI _healthText = null;
        [SerializeField] private Slider _healthSlider = null;
        private int _teamAKills = 0;
        private int _teamBKills = 0;
        private Color _friendlyColor;
        private Color _enemyColor;
        private TeamType _friendlyTeamType;

        private void Start()
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

            PlayerManager.Instance.OnPlayerTeamTypeSet += ShowTeamScore;
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

        private void ShowTeamScore()
        {
            _teamScore.SetActive(true);
            _teamAScoreText.text = "Team A: 0";
            _teamBScoreText.text = "Team B: 0";

            if (PlayerManager.Instance.FriendlyTeam == TeamType.TeamA)
            {
                _teamAScoreText.color = PlayerManager.Instance.FriendlyColor;
                _teamBScoreText.color = PlayerManager.Instance.EnemyColor;
            }
            else 
            {
                _teamAScoreText.color = PlayerManager.Instance.EnemyColor;
                _teamBScoreText.color = PlayerManager.Instance.FriendlyColor;
            }
        }

        public void ShowSoloScore()
        {
            _soloScore.SetActive(true); 
            _soloScoreText.text = "Kill: 0";
        }

        public int IncreaseTeamScore(TeamType team)
        {
            if (team == TeamType.TeamA)
            {
                _teamAKills++;
                _teamAScoreText.text = "Team A: " + _teamAKills.ToString();
                return _teamAKills;
            }
            else
            {
                _teamBKills++;
                _teamBScoreText.text = "Team B: " + _teamBKills.ToString();
                return _teamBKills;
            }
        }

        public void UpdateSoloScore(int kills) => _soloScoreText.text = "Kills: " + kills.ToString();

        public void UpdateHealthBar(int health, float healthPercentage)
        {
            _healthText.text = health.ToString();
            _healthSlider.value = healthPercentage;
        }
    }
}
