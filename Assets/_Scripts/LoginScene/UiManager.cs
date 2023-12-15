using SpellSlinger.Networking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;

namespace SpellFlinger.LoginScene
{
    public class UiManager : MonoBehaviour
    {
        [SerializeField] private TMP_InputField _playerName = null;
        [SerializeField] private Button _loginButton = null;
        [SerializeField] private TextMeshProUGUI _notificationText = null;

        private void Awake()
        {
            _loginButton.onClick.AddListener(LoginPressed);
        }

        private void LoginPressed()
        {
            if (_playerName.text.Length < 5)
            {
                _notificationText.text = "Your name must be at least 5 characters long.";
                _notificationText.gameObject.SetActive(true);
                return;
            }
            else
            {
                _notificationText.gameObject.SetActive(false);
            }

            _loginButton.interactable = false;
            _playerName.interactable = false;
            FusionConnection.Instance.StartGame(_playerName.text);
        }
    }
}
