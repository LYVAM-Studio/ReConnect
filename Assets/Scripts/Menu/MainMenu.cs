using UnityEngine;

namespace Reconnect.Menu
{
    public class MenuController : MonoBehaviour
    {
        public GameObject mainMenuCanvas;
        public GameObject multiplayerMenuCanvas;

        public void ShowMultiplayerMenu()
        {
            mainMenuCanvas.SetActive(false);
            multiplayerMenuCanvas.SetActive(true);
        }

        public void ShowMainMenu()
        {
            multiplayerMenuCanvas.SetActive(false);
            mainMenuCanvas.SetActive(true);
        }

        public void QuitGame()
        {
            Application.Quit();
        }
    }
}