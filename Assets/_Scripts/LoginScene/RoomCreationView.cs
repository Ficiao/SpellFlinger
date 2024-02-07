using SpellFlinger.Enum;
using SpellFlinger.Scriptables;
using SpellSlinger.Networking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpellFlinger.LoginScene
{
    public class RoomCreationView : MonoBehaviour
    {
        [SerializeField] private Toggle _teamDeathMatchToggle = null;
        [SerializeField] private Toggle _deathMatchToggle = null;
        [SerializeField] private TMP_InputField _roomNameInput = null;
        [SerializeField] private Button _returnButton = null;
        [SerializeField] private Button _createRoomButton = null;
        [SerializeField] private LevelSelectionToggle _levelSelectionTogglePrefab = null;
        [SerializeField] private ToggleGroup _levelSelectionContainer = null;
        [SerializeField] private GameObject _sessionView = null;
        private LevelType _selectedLevelType;

        private void Awake()
        {
            foreach(var data in LevelDataScriptable.Instance.Levels)
            {
                LevelSelectionToggle selectionToggle = Instantiate(_levelSelectionTogglePrefab, _levelSelectionContainer.transform);
                selectionToggle.ShowLevel(data.LevelType, _levelSelectionContainer, data.LevelImage, (levelType) => _selectedLevelType = levelType);
            }

            _returnButton.onClick.AddListener(() => 
            {
                _sessionView.SetActive(true);
                gameObject.SetActive(false);
            });

            _createRoomButton.onClick.AddListener(CreateRoom);
        }

        private void CreateRoom()
        {
            GameModeType gameMode = _teamDeathMatchToggle.isOn ? GameModeType.TDM : GameModeType.DM;
            LevelType level = _selectedLevelType;
            FusionConnection.Instance.JoinSession(_roomNameInput.text, gameMode, level);
        }
    }
}
