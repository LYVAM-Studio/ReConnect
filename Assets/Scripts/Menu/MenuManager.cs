using System;
using System.Collections;
using Mirror;
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
        [NonSerialized] public GameObject BreadBoardUI;
        
        [Header("Multiplayer parameters")]
        
        public ReconnectNetworkManager networkManager;
        public TMP_InputField serverAddress;
        
        public Menu CurrentMenu { get; private set; }
        public CursorState CurrentCursorState { get; private set; }
        public bool IsPlayerLocked { get; private set; }
        public PlayMode GameMode { get; private set; }
        public bool IsInGame { get; private set; }
        
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
            SetMenuTo(Menu.Main, CursorState.Shown, forceClearHistory:true);
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
            if (!_history.IsEmpty())
            {
                BackToPreviousMenu();
            }
            else if (CurrentMenu is Menu.None)
            {
                LockPlayer();
                SetMenuTo(Menu.Pause, CursorState.Shown);
            }
            else if (CurrentMenu is Menu.Main)
            {
                SetMenuTo(Menu.Quit, CursorState.Shown);
            }
        }
        
        private void OnToggleLessonsMenu(InputAction.CallbackContext ctx)
        {
            if (CurrentMenu is Menu.None)
            {
                LockPlayer();
                SetMenuTo(Menu.Lessons, CursorState.Shown);
            }
            else if (CurrentMenu is Menu.Lessons)
            {
                UnLockPlayer();
                BackToPreviousMenu();
            }
        }
        
        private void SetMenuTo(Menu menu, CursorState cursorState, bool forceClearHistory = false)
        {
            if (forceClearHistory)
                _history.Clear();
            else
                _history.Push(CurrentMenu, CurrentCursorState);
                
            mainMenu.SetActive(menu is Menu.Main);
            singleplayerMenu.SetActive(menu is Menu.Singleplayer);
            multiplayerMenu.SetActive(menu is Menu.Multiplayer);
            settingsMenu.SetActive(menu is Menu.Settings);
            pauseMenu.SetActive(menu is Menu.Pause);
            lessonsMenu.SetActive(menu is Menu.Lessons);
            imageViewerMenu.SetActive(menu is Menu.ImageViewer);
            knockOutMenu.SetActive(menu is Menu.KnockOut);
            connectionFailed.SetActive(menu is Menu.ConnectionFailed);
            quitMenu.SetActive(menu is Menu.Quit);
            BreadBoardUI?.SetActive(menu is Menu.BreadBoard);
            
            CurrentMenu = menu;
            
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

            (CurrentMenu, CurrentCursorState) = _history.Pop();
            
            mainMenu.SetActive(CurrentMenu is Menu.Main);
            singleplayerMenu.SetActive(CurrentMenu is Menu.Singleplayer);
            multiplayerMenu.SetActive(CurrentMenu is Menu.Multiplayer);
            settingsMenu.SetActive(CurrentMenu is Menu.Settings);
            pauseMenu.SetActive(CurrentMenu is Menu.Pause);
            lessonsMenu.SetActive(CurrentMenu is Menu.Lessons);
            imageViewerMenu.SetActive(CurrentMenu is Menu.ImageViewer);
            knockOutMenu.SetActive(CurrentMenu is Menu.KnockOut);
            connectionFailed.SetActive(CurrentMenu is Menu.ConnectionFailed);
            quitMenu.SetActive(CurrentMenu is Menu.Quit);
            BreadBoardUI?.SetActive(CurrentMenu is Menu.BreadBoard);
            
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
            
            if (CurrentMenu is Menu.None)
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
            SetMenuTo(Menu.ImageViewer, CursorState.Shown);
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
            SetMenuTo(Menu.KnockOut, CursorState.Locked);
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
        public void SetMenuToSingleplayer() => SetMenuTo(Menu.Singleplayer, CursorState.Shown);
        public void SetMenuToMultiplayer() => SetMenuTo(Menu.Multiplayer, CursorState.Shown);
        public void SetMenuToSettings() => SetMenuTo(Menu.Settings, CursorState.Shown);
        public void SetMenuToQuit() => SetMenuTo(Menu.Quit, CursorState.Shown);
        
        public void RunSingleplayerMode()
        {
            SetMenuTo(Menu.None, CursorState.Locked, forceClearHistory: true);
            GameMode = PlayMode.Single;
            IsInGame = true;
            networkManager.maxConnections = 1;
            networkManager.StartHost();
        }
        
        public void RunHostMode()
        {
            SetMenuTo(Menu.None, CursorState.Locked, forceClearHistory: true);
            GameMode = PlayMode.MultiHost;
            IsInGame = true;
            networkManager.StartHost();
        }
        
        public async void RunMultiplayerMode()
        {
            try
            {
                networkManager.networkAddress = serverAddress.text;
                bool success = await networkManager.StartClientAsync();
                if (success)
                {
                    SetMenuTo(Menu.None, CursorState.Locked, forceClearHistory: true);
                    GameMode = PlayMode.MultiServer;
                    IsInGame = true;
                }
                else
                {
                    SetMenuTo(Menu.ConnectionFailed, CursorState.Shown);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"An exception has been thrown while trying to connect:\n{e}");
                SetMenuTo(Menu.ConnectionFailed, CursorState.Shown);
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
            
            SetMenuTo(Menu.Main, CursorState.Shown, forceClearHistory:true);
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