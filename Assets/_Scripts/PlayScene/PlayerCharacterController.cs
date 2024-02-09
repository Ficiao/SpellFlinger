using Fusion;
using SpellFlinger.Enum;
using SpellFlinger.Scriptables;
using System;
using System.Collections;
using UnityEngine;

namespace SpellFlinger.PlayScene
{
    public class PlayerCharacterController : NetworkBehaviour
    {
        [SerializeField] private float _angularSpeed = 0;
        [SerializeField] private Transform _cameraEndTarget = null;
        [SerializeField] private Transform _cameraStartTarget = null;
        [SerializeField] private Transform _cameraAimTarget = null;
        [SerializeField] private float _gravityBurst = 0;
        [SerializeField] private CharacterController _controller;
        [SerializeField] private float _gravity = -9.81f;
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private float _jumpSpeed = 5f;
        [SerializeField] private int _respawnTime = 0;
        [SerializeField] private float _slopeRaycastDistance = 0f;
        [SerializeField] private Transform _shootOrigin;
        [SerializeField] private PlayerStats _playerStats = null;
        [SerializeField] private GameObject _playerModel = null;
        [SerializeField] private Transform _modelLeftHand = null;
        [SerializeField] private Transform _modelRightHand = null;
        [SerializeField] private Animator _playerAnimator = null;
        [SerializeField] private float _slowAmount = 0f;
        [SerializeField] private float _doubleJumpDelay = 0f;
        [SerializeField] private float _doubleJumpBoost = 1f;
        private bool[] inputs = null;
        private float yVelocity = 0;
        private float _yRotation = 0;
        private CameraController _cameraController = null;
        private Projectile _projectilePrefab = null;
        private PlayerAnimationState _playerAnimationState = PlayerAnimationState.Idle;
        private int _updatesSinceLastGrounded = 0;
        [SerializeField] private float _fireRate = 0;
        private float _fireCooldown = 0;
        private bool _respawnReady = false;
        private bool _doubleJumpAvailable = false;
        private float _jumpTime = 0;
        private IEnumerator _respawnCoroutine = null;
        private float _squareRootOfTwo = (float)Math.Sqrt(2);

        public PlayerStats PlayerStats => _playerStats;

        public override void Spawned()
        {
            if (HasStateAuthority) Initialize(1);
        }

        private void Initialize(int id)
        {
            _cameraController = CameraController.Instance;
            _cameraController.transform.parent = _cameraEndTarget;
            _cameraController.Init(_cameraStartTarget, _cameraEndTarget);
            _controller.enabled = false;
            transform.position = SpawnLocationManager.Instance.GetRandomSpawnLocation();
            _controller.enabled = true;
            _projectilePrefab = WeaponDataScriptable.Instance.GetWeaponData(WeaponDataScriptable.SelectedWeaponType).WeaponPrefab;
            inputs = new bool[5];
            PlayerAnimationController.Init(ref _playerAnimationState, _playerAnimator);
        }

        public void SetGloves(GameObject glovesPrefab, Vector3 position, float fireRate)
        {
            Instantiate(glovesPrefab, _modelLeftHand).transform.localPosition = position;
            Instantiate(glovesPrefab, _modelRightHand).transform.localPosition = position;
            _fireRate = fireRate;
        }

        public void Update()
        {
            /*
             * U ovoj metodi je potrebno spremiti unos tipki za kretanje naprijed, nazad, lijevo, desno i skok 
             * u lokalnu varijablu unosa. Također, ako je privatna varijabla instance CameraController, koja određuje je li 
             * kontroller aktivan ili ne, postavljena na aktivno stanje, potrebno je promijeniti lokalnu varijablu y rotacije
             * ovisno i pomaku miša, osjetljivosti iz SensitivitySettingsScriptable instance i proteklog vremena.
             * Ako je pritisnuta tipka za pucanje i prošlo je dovoljno vremena od prošlog pucanja, potrebno je pozvati metodu za pucanje i 
             * ažurirati vrijeme posljednjeg pucanja.
             */
        }

        private void Shoot()
        {
            PlayerAnimationController.PlayShootAnimation(_playerAnimator);

            Projectile projectile = Runner.Spawn(_projectilePrefab, _shootOrigin.position, inputAuthority: Runner.LocalPlayer);
            RaycastHit[] hits = Physics.RaycastAll(_cameraController.transform.position, _cameraController.transform.forward);
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.tag == "Projectile") continue;

                Vector3 shootDirection;
                if (Vector3.Dot(hit.point - _shootOrigin.position, transform.forward) >= 0)
                {
                    shootDirection = hit.point - _shootOrigin.position;
                    Debug.Log("Proper shoot");
                }
                else
                {
                    shootDirection = _cameraAimTarget.position - _shootOrigin.position;
                    Debug.Log("Bad shoot");
                }

                projectile.Throw(shootDirection, Runner.LocalPlayer, _playerStats);
                break;
            }
        }

        public override void FixedUpdateNetwork()
        {
            if(_respawnReady) Respawn();

            bool isGrounded = _controller.isGrounded;
            if (isGrounded) _updatesSinceLastGrounded = 0;
            else if (_updatesSinceLastGrounded < 2) 
            {
                isGrounded = true;
                _updatesSinceLastGrounded++;
            }

            if (_playerStats.Health <= 0 || _controller.enabled == false) return;

            Vector2 _inputDirection = Vector2.zero;
            if (inputs[0]) _inputDirection.y += 1;
            if (inputs[1]) _inputDirection.y -= 1;
            if (inputs[2]) _inputDirection.x -= 1;
            if (inputs[3]) _inputDirection.x += 1;

            PlayerAnimationController.AnimationUpdate(isGrounded, (int)_inputDirection.x, (int)_inputDirection.y, ref _playerAnimationState, _playerAnimator, _playerModel.transform, transform);
            Move(_inputDirection, isGrounded);
        }

        private void Move(Vector2 _inputDirection, bool isGroundedFrameNormalized)
        {
            transform.eulerAngles = new Vector3(0f, _yRotation, 0f);
            Vector3 _moveDirection = Vector3.zero;
            if(isGroundedFrameNormalized) _doubleJumpAvailable = true;

            /*
             * U ovoj metodu obavlja se mijenjanje pozicije lika. 
             * U nastavku je dan opis zamišljene logike kretanja, no ako želite, možete implementirati vlastitu logiku kretanja.
             * 
             * Prvo se određuje horizontalni pomak. Smjer horizontalnog kretanja se određuje s pomoću varijable _input 
             * gdje je prvi član vektora smjer lijevo-desno, a drugi naprijed nazad. Dobiveni smier se potom množi 
             * proteklim vremenom (pošto se radi o metodi FixedUpdateNetwork, a ne FixedUpdate, ne koristi se Time.FixedDeltaTime, 
             * nego Runner.DeltaTime), brzinom kretanja i u slučaju da je lik u usporenom stanju i koeficijentom usporenosti.
             * Na kraju, ako je aktivno kretanje u oba smjera vektor smjera je dijagonalan, te se iz toga razloga mora podijeliti 
             * sa korijenom od 2 kako ne bi došlo do ubrzanja. Korjenovanje je skupa akcija, stoga se koristi cache-irana lokalna 
             * varijabla _squareRootOfTwo.
             * 
             * Nakon toga je potrebno odrediti vertikalnu brzinu. Prvo se od lokalne varijable vertikalne brzine oduzima
             * vrijednost gravitacije pomnožene proteklim vremenom. Ako je nakon toga vrijednost vertikalne brzine približno 
             * jednaka nuli, to znači da je lik dosegnuo vrhunac skoka, te se dodaje burst gravitacije kako bi lik brže krenuo 
             * padati, što ima za posljedicu bolje korisničko iskustvo kretanja, u suprotnom se postiže osjećaj "Moon Gravity"
             * slučaja. 
             * 
             * Ako je lik prizemljen (_controller.isGrounded) vertikalna brzina mu se postavlja na nula, te u slučaju da je 
             * unesena komanda za skok vertikalna brzina se postavlja na brzinu skoka i vrijeme zadnjeg skoka se postavlja na 
             * trenutno vrijeme.
             * 
             * Ako lik nije prizemljen, a dana je naredba skoka, provjerava se je li zastavica mogućnosti duplog skoka postavljena 
             * na aktivno stanje i je li prošlo dovoljno vremena od prošlog skoka. Ako je, miče se zastavica duplog skoka i vertikalna brzina
             * se postavlja na brzinu skoka pomnoženu boostom za dupli skok.
             */

            _moveDirection = AdjustVelocityToSlope(_moveDirection);
            _moveDirection.y += yVelocity;
            _controller.Move(_moveDirection);
        }      

        private Vector3 AdjustVelocityToSlope(Vector3 velocity)
        {
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, _slopeRaycastDistance) && hit.collider.tag == "Ground")
            {
                Quaternion slopeRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                Vector3 adjustedVelocity = slopeRotation * velocity;

                if (adjustedVelocity.y < 0) return adjustedVelocity;
            }

            return velocity;
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void DisableControllerRpc()
        {
            if (HasStateAuthority) return;
            _controller.enabled = false;
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void EnableControllerRpc()
        {
            if (HasStateAuthority) return;
            _controller.enabled = true;
        }

        public void GameEnd(TeamType winnerTeam, Color winnerColor)
        {
            UiManager.Instance.HideDeathTimer();
            UiManager.Instance.ShowEndGameScreen(winnerTeam, winnerColor);
            if (_respawnCoroutine != null)
            {
                StopCoroutine(_respawnCoroutine);
                _respawnCoroutine = null;
            }
            StartCoroutine(GameEndCountdown());
        }

        public void GameEnd(string winnerName, Color winnerColor)
        {
            UiManager.Instance.HideDeathTimer();
            UiManager.Instance.ShowEndGameScreen(winnerName, winnerColor);
            if (_respawnCoroutine != null)
            {
                StopCoroutine(_respawnCoroutine);
                _respawnCoroutine = null;
            }
            StartCoroutine(GameEndCountdown());
        }

        private IEnumerator GameEndCountdown()
        {
            /*
             * Na početku je potrebno postaviti privatnu varijablu instance CameraController, koja označuje je li
             * kontroler aktivan ili ne, na neaktivno stanje i onesposobiti CharacterControler lika.
             * Nakon toga korutina čeka neki broj sekundi, nakon kojih naznačuje da je lik spreman za respawn, poziva metodu 
             * UiManager-a da makne ekran kraja igre i poziva metodu PlayerManager-a koja resetira sve bodove igre.
             * Liniju return null; je potrebno zmjeniti traženom logikom.
             */

            return null;
        }

        public void PlayerKilled()
        {
            PlayerAnimationController.SetDeadState(ref _playerAnimationState, _playerAnimator);
            _respawnCoroutine = RespawnCD();
            StartCoroutine(_respawnCoroutine);
        }

        private IEnumerator RespawnCD()
        {
            /*
             * U ovoj metodi potrebno je aktivirati ekran smrti pozivom metode instance UiManager-a, postaviti privatnu varijablu instance CameraController, 
             * koja označuje je li kontroler aktivan ili ne, na neaktivno stanje i onesposobiti CharacterControler lika. Također je potrebno pozvati 
             * metode udaljenih procedura ove instance na drugim računalima koje onesposobljuju CharacterController kako bi se iskuljučio Collider i lokalno 
             * i na drugim klijentima. Potom je potrebno čekati određeni broj sekundi, i svake sekunde ažurirati timer ekrana smrti.
             * Nakon što istekne traženo vrijeme potrebno je naznačiti da je lik spreman za respawn.
             * Potrebno je zamijeniti liniju return null; traženom logikom.
             */

            return null;
        }

        private void Respawn()
        {
            /*
             * U ovoj metodi je potrebno postaviti sve vrijednosti potrebne za ponovnu aktivaciju lika.
             * Potrebno je ugasiti zastavicu spremnosti za respawn, resetirati životne bodove, maknuti ekran smrti,
             * postaviti poziciju na nasumičnu poziciju kao u metodi Spwaned, pozvati metodu udaljene procedure za 
             * aktivaciju CharacterController komponente na instanci ovog objekta na drugim klijentima i aktivirati 
             * lokalni CharacterController.
             */

            _cameraController.CameraEnabled = true;
        }
    }
}