using Mirror;
using Player;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Scene
{
    public class SceneScript : NetworkBehaviour
    {
        public Text canvasStatusText;
        [FormerlySerializedAs("playerScript")] public PlayerNetwork playerNetwork;

        [SyncVar(hook = nameof(OnStatusTextChanged))]
        public string statusText;

        void OnStatusTextChanged(string _Old, string _New)
        {
            //called from sync var hook, to update info on screen for all players
            canvasStatusText.text = statusText;
        }

        public void ButtonSendMessage()
        {
            if (playerNetwork != null)  
                playerNetwork.CmdSendPlayerMessage();
        }
    }
}