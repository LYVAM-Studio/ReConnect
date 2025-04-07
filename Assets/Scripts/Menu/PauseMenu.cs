using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace Reconnect.Menu
{
    public class PauseMenu : MonoBehaviour
    {
        public GameObject pauseMenuUI;
        public static bool IsPaused = false;

        private PlayerInput playerInput;

        void Start()
        {
            pauseMenuUI.SetActive(false);
            playerInput = FindFirstObjectByType<PlayerInput>();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (IsPaused)
                {
                    Resume();
                }
                else
                {
                    Pause();
                }
            }
        }

        public void Resume()
        {
            pauseMenuUI.SetActive(false);
            IsPaused = false;
            if (playerInput != null)
            {
                playerInput.actions["Move"].Enable();
            }
        }

        void Pause()
        {
            pauseMenuUI.SetActive(true);
            IsPaused = true;
            if (playerInput != null)
            {
                playerInput.actions["Move"].Disable();
            }
        }

        public void QuitGame()
        {
            Application.Quit();
        }
    }
}