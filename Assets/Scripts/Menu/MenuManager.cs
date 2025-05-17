using System;
using System.Collections;
using Mirror;
using Reconnect.Electronics.Breadboards;
using Reconnect.Menu.Lessons;
using Reconnect.Player;
using Reconnect.Utils;
using TMPro;
using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

namespace Reconnect.Menu
{
    public class MenuManager : MonoBehaviour
    {
        public static MenuManager Instance;
        
        [Header("Menu canvas")]
        
        public GameObject mainMenu;
        public GameObject singleplayerMenu;
        public GameObject multiplayerMenu;
        public GameObject settingsMenu;
        public GameObject pauseMenu;
        public GameObject lessonsMenu;
        public GameObject imageViewerMenu;
        public GameObject knockOutMenu;
        [VolumeComponent.Indent] public TMP_Text timerText;
        public GameObject connectionFailed;
        public GameObject quitMenu;
        [NonSerialized] public BreadboardHolder BreadBoardHolder;
        
        [Header("Multiplayer parameters")]
        
        public ReconnectNetworkManager networkManager;
        public TMP_InputField hostPort;
        public TMP_InputField serverAddress;
        public TMP_InputField serverPort;
        
        public MenuState CurrentMenuState { get; private set; }
        public CursorState CurrentCursorState { get; private set; }
        public bool IsPlayerLocked { get; private set; }
        public PlayMode GameMode { get; private set; }

        private readonly History _history = new();
        private PlayerControls _controls;
        
        private void Awake()
        {
            if (Instance is not null)
                throw new Exception("A MenuController has already been instantiated.");
            
            Instance = this;

            _controls = new PlayerControls();
            _controls.Menu.Esc.performed += OnEscPressed;
            _controls.Menu.Lessons.performed += OnToggleLessonsMenu;
            
            SetMenuTo(MenuState.Main, CursorState.Shown, forceClearHistory:true);
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
            _controls.Menu.Lessons.performed -= OnToggleLessonsMenu;
        }

        private void OnEscPressed(InputAction.CallbackContext ctx)
        {
            if (CurrentMenuState is MenuState.KnockOut) return;
            
            if (!_history.IsEmpty())
            {
                if (CurrentMenuState is MenuState.BreadBoard)
                    BreadBoardHolder?.Activate(false);
                
                BackToPreviousMenu();
            }
            else
            {
                if (CurrentMenuState is MenuState.None)
                {
                    LockPlayer();
                    SetMenuTo(MenuState.Pause, CursorState.Shown);
                }
                else if (CurrentMenuState is MenuState.Main)
                {
                    SetMenuTo(MenuState.Quit, CursorState.Shown);
                }
                else
                {
                    throw new UnreachableCaseException(
                        $"Menu {CurrentMenuState} should not be active with an empty history.");
                }
            }
        }
        
        private void OnToggleLessonsMenu(InputAction.CallbackContext ctx)
        {
            if (CurrentMenuState is MenuState.None)
            {
                LockPlayer();
                SetMenuTo(MenuState.Lessons, CursorState.Shown);
            }
            else if (CurrentMenuState is MenuState.Lessons)
            {
                UnLockPlayer();
                BackToPreviousMenu();
            }
        }
        
        public void SetMenuTo(MenuState menu, CursorState cursorState, bool forceClearHistory = false)
        {
            if (forceClearHistory)
                _history.Clear();
            else
                _history.Push(CurrentMenuState, CurrentCursorState);
                
            mainMenu.SetActive(menu is MenuState.Main);
            singleplayerMenu.SetActive(menu is MenuState.Singleplayer);
            multiplayerMenu.SetActive(menu is MenuState.Multiplayer);
            settingsMenu.SetActive(menu is MenuState.Settings);
            pauseMenu.SetActive(menu is MenuState.Pause);
            lessonsMenu.SetActive(menu is MenuState.Lessons);
            imageViewerMenu.SetActive(menu is MenuState.ImageViewer);
            knockOutMenu.SetActive(menu is MenuState.KnockOut);
            connectionFailed.SetActive(menu is MenuState.ConnectionFailed);
            quitMenu.SetActive(menu is MenuState.Quit);
            BreadBoardHolder?.Activate(menu is MenuState.BreadBoard);
            
            CurrentMenuState = menu;
            
            if (cursorState is CursorState.Shown)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.Confined;
            }
            else
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }

            CurrentCursorState = cursorState;
        }

        public void BackToPreviousMenu()
        {
            if (_history.IsEmpty())
                throw new ArgumentException("Cannot go back with an empty history");

            (CurrentMenuState, CurrentCursorState) = _history.Pop();
            
            mainMenu.SetActive(CurrentMenuState is MenuState.Main);
            singleplayerMenu.SetActive(CurrentMenuState is MenuState.Singleplayer);
            multiplayerMenu.SetActive(CurrentMenuState is MenuState.Multiplayer);
            settingsMenu.SetActive(CurrentMenuState is MenuState.Settings);
            pauseMenu.SetActive(CurrentMenuState is MenuState.Pause);
            lessonsMenu.SetActive(CurrentMenuState is MenuState.Lessons);
            imageViewerMenu.SetActive(CurrentMenuState is MenuState.ImageViewer);
            knockOutMenu.SetActive(CurrentMenuState is MenuState.KnockOut);
            connectionFailed.SetActive(CurrentMenuState is MenuState.ConnectionFailed);
            quitMenu.SetActive(CurrentMenuState is MenuState.Quit);
            BreadBoardHolder?.Activate(CurrentMenuState is MenuState.BreadBoard);
            
            if (CurrentCursorState is CursorState.Shown)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.Confined;
            }
            else
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
            
            if (CurrentMenuState is MenuState.None)
                UnLockPlayer();
        }

        public void LockPlayer(bool locked)
        {
            if (IsPlayerLocked == locked) return;

            if (!NetworkClient.localPlayer.TryGetComponent(out PlayerMovementsNetwork player))
                throw new ComponentNotFoundException("No PlayerMovementsNetwork has been found on the local player.");

            if (locked)
            {
                player.isLocked = true;
                FreeLookCamera.InputAxisController.enabled = false;
            }
            else
            {
                player.isLocked = false;
                FreeLookCamera.InputAxisController.enabled = true;
            }

            IsPlayerLocked = locked;
        }
        
        public void OpenImageInViewer(Sprite sprite)
        {
            SetMenuTo(MenuState.ImageViewer, CursorState.Shown);
            ImageViewerManager.Instance.LoadImage(sprite);
        }
        
        public void CloseImageViewer()
        {
            BackToPreviousMenu();
            ImageViewerManager.Instance.CloseImage();
        }

        public IEnumerator KnockOutForSeconds(uint seconds)
        {
            // the player is locked from the call
            SetMenuTo(MenuState.KnockOut, CursorState.Locked);
            
            for (uint i = seconds; i > 0; i--)
            {
                timerText.text = i.ToString();
                yield return new WaitForSeconds(1);
            }
            
            BackToPreviousMenu();
            // the player is unlocked after the call
        }

        public void LockPlayer() => LockPlayer(true);
        public void UnLockPlayer() => LockPlayer(false);
        public void SetMenuToSingleplayer() => SetMenuTo(MenuState.Singleplayer, CursorState.Shown);
        public void SetMenuToMultiplayer() => SetMenuTo(MenuState.Multiplayer, CursorState.Shown);
        public void SetMenuToSettings() => SetMenuTo(MenuState.Settings, CursorState.Shown);
        public void SetMenuToQuit() => SetMenuTo(MenuState.Quit, CursorState.Shown);
        
        public void RunSingleplayerMode()
        {
            SetMenuTo(MenuState.None, CursorState.Locked, forceClearHistory: true);
            GameMode = PlayMode.Single;
            networkManager.maxConnections = 1;
            ReconnectNetworkManager.SetConnectionPort(7777);
            networkManager.StartHost();
        }
        
        public void RunHostMode()   
        {
            if (!ushort.TryParse(hostPort.text, out ushort port))
            {
                SetMenuTo(MenuState.ConnectionFailed, CursorState.Shown);
                return;
            }
            
            SetMenuTo(MenuState.None, CursorState.Locked, forceClearHistory: true);
            GameMode = PlayMode.MultiHost;
            ReconnectNetworkManager.SetConnectionPort(port);
            networkManager.StartHost();
        }
        
        public async void RunMultiplayerMode()
        {
            if (!ushort.TryParse(hostPort.text, out ushort port))
            {
                SetMenuTo(MenuState.ConnectionFailed, CursorState.Shown);
                return;
            }
            
            try
            {
                networkManager.networkAddress = serverAddress.text;
                ReconnectNetworkManager.SetConnectionPort(port);
                
                bool success = await networkManager.StartClientAsync();
                if (success)
                {
                    SetMenuTo(MenuState.None, CursorState.Locked, forceClearHistory: true);
                    GameMode = PlayMode.MultiServer;
                }
                else
                {
                    SetMenuTo(MenuState.ConnectionFailed, CursorState.Shown);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"An exception has been thrown while trying to connect:\n{e}");
                SetMenuTo(MenuState.ConnectionFailed, CursorState.Shown);
            }
        }

        public void StopRunning()
        {
            if (GameMode is PlayMode.MultiServer)
                networkManager.StopClient();
            else
                networkManager.StopHost();

            // if a temporary cam (bb holder cam), then reset it
            var currentCam = CinemachineCore.GetVirtualCamera(0);
            if (currentCam.Priority == 2)
                currentCam.Priority = 0;
            FreeLookCamera.InputAxisController.enabled = true;
            
            SetMenuTo(MenuState.Main, CursorState.Shown, forceClearHistory:true);
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