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
            /*
             * Metodu je potrebno nadopuniti s kodom za stvaranje Toggle objekata za izbor scene.
             * Popis podataka o scenama se može dobiti iz instance LevelDataScriptable klase.
             * Stvaranje i inicijalizaciju objekata se provodi na sličan način kao stvaranje
             * Toggle objekata za izbor oružja u Awake metodi SessionView klase.
             * 
             * Nakon toga je potrebno inicijalizirati callback Return gumba da na pritisak gasi 
             * izbornik stvaranja sobe i otvara izbornik odabira otvorenih soba.
             * Također potrebno je inicijalizirati callback Create Room gumba da na pritisak
             * poziva metodu za stvaranje sobe.
             */
        }

        private void CreateRoom()
        {
            /*  
             *  Potrebno je pozvati metodu instance FusionConnection za stvaranje sobe, te joj poslati potrebne parametre.
             */
        }
    }
}
