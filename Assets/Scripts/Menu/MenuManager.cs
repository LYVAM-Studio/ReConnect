using System;
using Mirror;
using Reconnect.Player;
using TMPro;
using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Reconnect.Menu
{
    public class MenuManager : MonoBehaviour
    {
        public enum MenuState { None, Main, Singleplayer, Multiplayer, Settings, Pause }
        public enum PlayMode { Single, MultiHost, MultiServer }
        
        public static MenuManager Instance;
        
        [Header("Multiplayer parameters")]
        public NetworkManager networkManager;
        public TMP_InputField serverAddress;
        [Header("Menu canvas")]
        public GameObject mainMenu;
        public GameObject singleplayerMenu;
        public GameObject multiplayerMenu;
        public GameObject settingsMenu;
        public GameObject pauseMenu;

        private PlayerControls _controls;
        private CinemachineInputAxisController _camInputAxis;
        private CinemachineInputAxisController CamInputAxis
        {
            get
            {
                if (_camInputAxis == null)
                    _camInputAxis = GameObject.FindGameObjectWithTag("freeLookCamera")
                        .GetComponent<CinemachineInputAxisController>();
                return _camInputAxis;
            }
        }
        
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
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                NetworkClient.localPlayer.GetComponent<PlayerMovementsNetwork>().isLocked = true;
                CamInputAxis.enabled = false;
            }
            else
            {
                // todo manage if paused when in an interface
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                NetworkClient.localPlayer.GetComponent<PlayerMovementsNetwork>().isLocked = false;
                CamInputAxis.enabled = true;
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
            GameMode = PlayMode.Single;
            CurrentMenu = MenuState.None;
            IsInGame = true;
            networkManager.maxConnections = 1;
            networkManager.StartHost();
        }
        
        public void RunHostMode()
        {
            GameMode = PlayMode.MultiHost;
            CurrentMenu = MenuState.None;
            IsInGame = true;
            networkManager.StartHost();
        }
        
        public void RunMultiplayerMode()
        {
            GameMode = PlayMode.MultiServer;
            CurrentMenu = MenuState.None;
            IsInGame = true;
            networkManager.networkAddress = serverAddress.text;
            networkManager.StartClient();
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

            CurrentMenu = MenuState.Main;
            IsInGame = false;
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