using System;
using System.Collections;
using Mirror;
using Reconnect.Player;
using Reconnect.Utils;
using TMPro;
using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Reconnect.Menu
{
    public class MenuManager : MonoBehaviour
    {
        public enum MenuState { None, Main, Singleplayer, Multiplayer, Settings, Pause }
        public enum PlayMode { Single, MultiHost, MultiServer }
        
        public static MenuManager Instance;
        
        [Header("Multiplayer parameters")]
        public ReconnectNetworkManager networkManager;
        public TMP_InputField serverAddress;
        [Header("Menu canvas")]
        public GameObject mainMenu;
        public GameObject singleplayerMenu;
        public GameObject multiplayerMenu;
        public GameObject settingsMenu;
        public GameObject pauseMenu;

        public GameObject errorBanner;             // UI panel or text background that represents the banner displayed in error case
        public TextMeshProUGUI errorBannerText;        // Error message text mesh
        public float errorDisplayDuration = 3f;    // How long the error bqnner stays visible

        private Coroutine _currentBannerRoutine;
        
        private PlayerControls _controls;

        private CursorLockMode _previousCursorLockMode;
        private bool _previousCursorVisibility;
        
        // Should not be directly used
        private MenuState _currentMenu;
        public MenuState CurrentMenu
        {
            get => _currentMenu;
            set
            {
                mainMenu.SetActive(value is MenuState.Main);
                singleplayerMenu.SetActive(value is MenuState.Singleplayer);
                multiplayerMenu.SetActive(value is MenuState.Multiplayer);
                settingsMenu.SetActive(value is MenuState.Settings);
                pauseMenu.SetActive(value is MenuState.Pause);
                _currentMenu = value;
            }
        }

        public bool IsInGame { get; private set; }
        public PlayMode GameMode { get; private set; }

        private void Awake()
        {
            if (Instance != null)
                throw new Exception("A MenuController has already been instantiated.");
            Instance = this;
            
            _controls = new PlayerControls();
            _controls.Menu.Esc.performed += OnEscPressed;
            
            CurrentMenu = MenuState.Main;
        }
        
        private void OnEnable()
        {
            _controls.Enable();
        }

        private void OnDisable()
        {
            _controls.Disable();
        }

        private void OnDestroy()
        {
            _controls.Menu.Esc.performed -= OnEscPressed;
        }

        private void OnEscPressed(InputAction.CallbackContext ctx)
        {
            if (!IsInGame)
            {
                SetMenuToMain();
            }
            else switch (CurrentMenu)
            {
                case MenuState.None:
                    SetLock(true);
                    CurrentMenu = MenuState.Pause;
                    break;
                case MenuState.Pause:
                    ClosePauseMenu();
                    break;
                default:
                    CurrentMenu = MenuState.Pause;
                    break;
            }
        }
        
        private void SetLock(bool value)
        {
            if (value)
            {
                _previousCursorVisibility = Cursor.visible;
                _previousCursorLockMode = Cursor.lockState;
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                if (!NetworkClient.localPlayer.TryGetComponent(out PlayerMovementsNetwork movements))
                    throw new ComponentNotFoundException(
                        "No PlayerMovementsNetwork component has been found on the local player.");
                movements.isLocked = true;
                FreeLookCamera.InputAxisController.enabled = false;
            }
            else
            {
                Cursor.visible = _previousCursorVisibility;
                Cursor.lockState = _previousCursorLockMode;
                if (!NetworkClient.localPlayer.TryGetComponent(out PlayerMovementsNetwork movements))
                    throw new ComponentNotFoundException(
                        "No PlayerMovementsNetwork component has been found on the local player.");
                movements.isLocked = true;
                FreeLookCamera.InputAxisController.enabled = true;
            }
        }
        
        public void SetMenuToMain()
        {
            CurrentMenu = MenuState.Main;
        }
        
        public void SetMenuToPrevious()
        {
            CurrentMenu = IsInGame ? MenuState.Pause : MenuState.Main;
        }
        
        public void SetMenuToSingleplayer() 
        {
            CurrentMenu = MenuState.Singleplayer;
        }
        
        public void SetMenuToMultiplayer() 
        {
            CurrentMenu = MenuState.Multiplayer;
        }
        
        public void SetMenuToSettings() 
        {
            CurrentMenu = MenuState.Settings;
        }
        
        public void ClosePauseMenu()
        {
            SetLock(false);
            CurrentMenu = MenuState.None;
        }

        public void RunSingleplayerMode()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            GameMode = PlayMode.Single;
            CurrentMenu = MenuState.None;
            IsInGame = true;
            networkManager.maxConnections = 1;
            networkManager.StartHost();
        }
        
        public void RunHostMode()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            GameMode = PlayMode.MultiHost;
            CurrentMenu = MenuState.None;
            IsInGame = true;
            networkManager.StartHost();
        }
        
        public async void RunMultiplayerMode()
        {
            networkManager.networkAddress = serverAddress.text;
            bool success = await networkManager.StartClientAsync();
            if (success)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                IsInGame = true;
                GameMode = PlayMode.MultiServer;
                CurrentMenu = MenuState.None;
            }
            else
            {
                ShowConnectionError("Connection failed. Please try again.");
            }
        }

        public void StopRunning()
        {
            switch (GameMode)
            {
                case PlayMode.MultiServer:
                    networkManager.StopClient();
                    break;
                default:
                    networkManager.StopHost();
                    break;
            }

            // if a temporary cam (bb holder cam), then reset it
            var currentCam = CinemachineCore.GetVirtualCamera(0);
            if (currentCam.Priority == 2)
                currentCam.Priority = 0;
            FreeLookCamera.InputAxisController.enabled = true;
            CurrentMenu = MenuState.Main;
            IsInGame = false;
        }
        
        public void ShowConnectionError(string message)
        {
            if (_currentBannerRoutine != null)
                StopCoroutine(_currentBannerRoutine);

            _currentBannerRoutine = StartCoroutine(ShowBannerRoutine(message));
        }

        private IEnumerator ShowBannerRoutine(string message)
        {
            errorBannerText.text = message;
            errorBanner.SetActive(true);

            yield return new WaitForSeconds(errorDisplayDuration);

            errorBanner.SetActive(false);
        }
        
        public void QuitGame()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else 
            Application.Quit();
#endif
        }
    }
}