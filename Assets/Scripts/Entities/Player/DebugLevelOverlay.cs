using Mirror;
using Reconnect.Player;
using TMPro;
using UnityEngine;

namespace Reconnect.Menu
{
    public class DebugLevelOverlay : NetworkBehaviour
    {
        [SerializeField] private TMP_Text levelText;
        private PlayerGetter _localPlayer;

        public override void OnStartLocalPlayer()
        {
            _localPlayer = GetComponent<PlayerGetter>();
            levelText = MenuManager.Instance.hudMenu.GetComponentInChildren<TMP_Text>();
        }

        void Update()
        {
            if (_localPlayer is null) return; // while the client is not set
            if (isClient)
            {
                Debug.Log($"Overlay using: {_localPlayer.gameObject.name}, level: {_localPlayer.Level.value}");
                levelText.text = $"Level: {_localPlayer.Level.value}";
            }
        }
    }
}