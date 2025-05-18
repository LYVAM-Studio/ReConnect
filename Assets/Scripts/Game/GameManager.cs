using System;
using Mirror;
using Reconnect.Menu;
using TMPro;
using UnityEngine;

namespace Reconnect.Game
{
    public class GameManager : NetworkBehaviour
    {
        [NonSerialized] 
        public static GameManager Instance;
        
        [SyncVar(hook = nameof(OnLevelChange))]
        [NonSerialized]
        public uint Level = 1;

        public void OnLevelChange(uint oldLevel, uint newLevel)
        {
            MenuManager.Instance.hudMenu.GetComponentInChildren<TMP_Text>().text = $"Level : {newLevel}";
        }
        
        private void Awake()
        {
            if (Instance is not null)
                throw new Exception("A GameManager has already been instantiated.");

            Instance = this;
        }
        
        public void LevelUp()
        {
            Level++;
            // TODO : display lesson of the new level
            // TODO : display indication on the new level
        }
        
    }
}