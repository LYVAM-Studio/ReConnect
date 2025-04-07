using Mirror;
using UnityEngine;
using UnityEngine.UI;

namespace Reconnect.Menu
{
    public class MultiplayerMenu : MonoBehaviour
    {
        public InputField ipInputField;
        public NetworkManager networkManager;
        public string gameSceneName = "GameScene";

        public void JoinGame()
        {
            networkManager.networkAddress =
                string.IsNullOrEmpty(ipInputField.text) ? "localhost" : ipInputField.text;
            networkManager.StartClient();
        }

        public void HostGame()
        {
            networkManager.onlineScene = gameSceneName;

            networkManager.StartHost();
        }
    }
}