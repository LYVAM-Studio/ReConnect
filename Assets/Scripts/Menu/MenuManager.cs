using System;
using System.Collections;
using Mirror;
using Reconnect.Electronics.Breadboards;
using Reconnect.Game;
using Reconnect.Menu.Lessons;
using Reconnect.Player;
using Reconnect.Utils;
using TMPro;
using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Reconnect.Menu
{
    public class MenuManager : MonoBehaviour
    {
        public static MenuManager Instance;
        
        [Header("Multiplayer parameters")]
        [SerializeField] private ReconnectNetworkManager networkManager;
        
        [Header("Menu canvas")]
        [SerializeField] private GameObject mainMenu;
        [SerializeField] private GameObject singleplayerMenu;
        [SerializeField] private GameObject multiplayerMenu;
        [SerializeField] private GameObject settingsMenu;
        [SerializeField] private GameObject pauseMenu;
        [SerializeField] private GameObject hudMenu;
        [SerializeField] private GameObject lessonsMenu;
        [SerializeField] private GameObject imageViewerMenu;
        [SerializeField] private GameObject knockOutMenu;
        [SerializeField] private GameObject connectionMenu;
        [SerializeField] private GameObject connectionFailed;
        [SerializeField] private GameObject quitMenu;
        [SerializeField] private GameObject newLessonMenu;
        
        [Header("Level Mission Briefs")]
        [SerializeField] private GameObject level1Menu;
        [SerializeField] private GameObject level2Menu;
        [SerializeField] private GameObject level3Menu;
        [SerializeField] private GameObject level4Menu;
        [SerializeField] private GameObject level5Menu;
        [SerializeField] private GameObject level6Menu;
        [SerializeField] private GameObject level7Menu;
        [SerializeField] private GameObject level8Menu;
        [SerializeField] private GameObject level9Menu;
        
        [Header("TextMeshPro references")]
        [SerializeField] private TMP_InputField hostPort;
        [SerializeField] private TMP_InputField serverAddress;
        [SerializeField] private TMP_InputField serverPort;
        [SerializeField] private TMP_Text knockOutReason;
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private TMP_Text errorMsg;
        [SerializeField] private TMP_Text hudLevelText;
        
        [Header("Other useful references")]
        [SerializeField] private NewLessonMenuController newLessonController;
        [NonSerialized] public BreadboardHolder BreadBoardHolder;
        
        
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
            _controls.Menu.MissionBrief.performed += OnToggleMissionBrief;
            
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
            _controls.Menu.MissionBrief.performed -= OnToggleMissionBrief;
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
        
        private void OnToggleMissionBrief(InputAction.CallbackContext ctx)
        {
            if (CurrentMenuState is MenuState.None)
            {
                LockPlayer();
                SetMenuToMissionBrief(GameManager.Instance.Level);
            }
            else if (CurrentMenuState.IsBriefMission())
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
            connectionMenu.SetActive(menu is MenuState.Connection);
            connectionFailed.SetActive(menu is MenuState.ConnectionFailed);
            quitMenu.SetActive(menu is MenuState.Quit);
            newLessonMenu.SetActive(menu is MenuState.NewLesson);
            BreadBoardHolder?.Activate(menu is MenuState.BreadBoard);
            level1Menu.SetActive(menu is MenuState.Level1);
            level2Menu.SetActive(menu is MenuState.Level2);
            level3Menu.SetActive(menu is MenuState.Level3);
            level4Menu.SetActive(menu is MenuState.Level4);
            level5Menu.SetActive(menu is MenuState.Level5);
            level6Menu.SetActive(menu is MenuState.Level6);
            level7Menu.SetActive(menu is MenuState.Level7);
            level8Menu.SetActive(menu is MenuState.Level8);
            level9Menu.SetActive(menu is MenuState.Level9);
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
            {
                Debug.LogWarning("Cannot go back with an empty history");
                return;
            }

            (CurrentMenuState, CurrentCursorState) = _history.Pop();
            
            mainMenu.SetActive(CurrentMenuState is MenuState.Main);
            singleplayerMenu.SetActive(CurrentMenuState is MenuState.Singleplayer);
            multiplayerMenu.SetActive(CurrentMenuState is MenuState.Multiplayer);
            settingsMenu.SetActive(CurrentMenuState is MenuState.Settings);
            pauseMenu.SetActive(CurrentMenuState is MenuState.Pause);
            lessonsMenu.SetActive(CurrentMenuState is MenuState.Lessons);
            imageViewerMenu.SetActive(CurrentMenuState is MenuState.ImageViewer);
            knockOutMenu.SetActive(CurrentMenuState is MenuState.KnockOut);
            connectionMenu.SetActive(CurrentMenuState is MenuState.Connection);
            connectionFailed.SetActive(CurrentMenuState is MenuState.ConnectionFailed);
            quitMenu.SetActive(CurrentMenuState is MenuState.Quit);
            newLessonMenu.SetActive(CurrentMenuState is MenuState.NewLesson);
            BreadBoardHolder?.Activate(CurrentMenuState is MenuState.BreadBoard);
            level1Menu.SetActive(CurrentMenuState is MenuState.Level1);
            level2Menu.SetActive(CurrentMenuState is MenuState.Level2);
            level3Menu.SetActive(CurrentMenuState is MenuState.Level3);
            level4Menu.SetActive(CurrentMenuState is MenuState.Level4);
            level5Menu.SetActive(CurrentMenuState is MenuState.Level5);
            level6Menu.SetActive(CurrentMenuState is MenuState.Level6);
            level7Menu.SetActive(CurrentMenuState is MenuState.Level7);
            level8Menu.SetActive(CurrentMenuState is MenuState.Level8);
            level9Menu.SetActive(CurrentMenuState is MenuState.Level9);
            
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

        public void SetKnockOutReason(string reason)
        {
            knockOutReason.text = $"{reason}\nWait until your regain consciousness...";
        }

        public void SetMenuToNewLesson(uint level, Sprite lesson)
        {
            LockPlayer();
            newLessonController.LoadImage(lesson);
            newLessonController.SetTextToLevel(level);
            FreeLookCamera.InputAxisController.enabled = false;
            SetMenuTo(MenuState.NewLesson, CursorState.Shown);
        }
        
        public void SetMenuToMissionBrief(uint level)
        {
            if (level == 0 || level > 9)
                throw new ArgumentOutOfRangeException(nameof(level));
            LockPlayer();
            FreeLookCamera.InputAxisController.enabled = false;
            SetMenuTo((MenuState)level, CursorState.Shown);
        }
        
        public void SetLevel(uint level)
        {
            hudLevelText.text = $"Level : {level}";
        }
        
        public void RunSingleplayerMode()
        {
            SetMenuTo(MenuState.Connection, CursorState.Shown);
            GameMode = PlayMode.Single;
            networkManager.maxConnections = 1;
            ReconnectNetworkManager.SetConnectionPort(7777);
            networkManager.StartHost();
            SetMenuTo(MenuState.None, CursorState.Locked, forceClearHistory: true);
        }
        
        public void RunHostMode()   
        {
            if (!ushort.TryParse(hostPort.text, out ushort port))
            {
                SetMenuTo(MenuState.ConnectionFailed, CursorState.Shown);
                errorMsg.text = "Invalid port\n\nTry again with a valid port";
                return;
            }
            
            SetMenuTo(MenuState.Connection, CursorState.Shown);
            GameMode = PlayMode.MultiHost;
            ReconnectNetworkManager.SetConnectionPort(port);
            networkManager.StartHost();
            SetMenuTo(MenuState.None, CursorState.Locked, forceClearHistory: true);
        }
        
        public async void RunMultiplayerMode()
        {
            if (!ushort.TryParse(serverPort.text, out ushort port))
            {
                SetMenuTo(MenuState.ConnectionFailed, CursorState.Shown);
                errorMsg.text = "Invalid port\n\nTry again with a valid port";
                return;
            }
            
            try
            {
                SetMenuTo(MenuState.Connection, CursorState.Shown);
                
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
                    BackToPreviousMenu();
                    SetMenuTo(MenuState.ConnectionFailed, CursorState.Shown);
                    errorMsg.text = "Connection failed\n\nPlease try again";
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"An exception has been thrown while trying to connect:\n{e}");
                BackToPreviousMenu();
                SetMenuTo(MenuState.ConnectionFailed, CursorState.Shown);
                errorMsg.text = "Connection failed\n\nPlease try again";
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