using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using Reconnect.Menu;
using Reconnect.Menu.Lessons;
using Reconnect.Utils;
using TMPro;
using UnityEngine;

namespace Reconnect.Game
{
    public class GameManager : NetworkBehaviour
    {
        [NonSerialized] 
        public static GameManager Instance;
        
        [SyncVar(hook = nameof(OnLevelChange))]
        public uint Level;

        private readonly List<Sprite> _lessonsByLevel = new();

        private LessonsInventoryManager _lessonsInventoryManager;
        
        // lessons to load in the order of the levels
        [SerializeField]
        private string[] lessonSpritesNames =
        {
            "Reading a circuit diagram",
            "Intensity in a circuit",
            "Tension in a circuit",
            "Resistance in a circuit",
            "Resistance color code"
        };
        
        public void OnLevelChange(uint oldLevel, uint newLevel)
        {
            StartCoroutine(WaitForLocalPlayerThenHandleLevelChange(oldLevel, newLevel));
        }

        private IEnumerator WaitForLocalPlayerThenHandleLevelChange(uint oldLevel, uint newLevel)
        {
            yield return new WaitUntil(() => NetworkClient.localPlayer is not null);
    
            if (newLevel == 0)
            {
                Debug.LogError("The new level is null and it should not be!");
                yield break;
            }
            // change the HUD level text
            MenuManager.Instance.SetLevel(newLevel);
            // show the canva of the mission brief for this level
            MenuManager.Instance.SetMenuToMissionBrief(newLevel);
            int lessonIndex = (int)newLevel - 1;
            // show the menu of the new lesson with the image of the lesson
            MenuManager.Instance.SetMenuToNewLesson(newLevel, _lessonsByLevel[lessonIndex]);
            for (uint level = oldLevel + 1; level <= newLevel; level++)
            {
                _lessonsInventoryManager.AddItem(_lessonsByLevel[(int)level - 1].name, _lessonsByLevel[(int)level - 1]);
            }
        }
        
        private void Awake()
        {
            if (Instance is not null)
                throw new Exception("A GameManager has already been instantiated.");
            Instance = this;
            if (!TryGetComponent(out _lessonsInventoryManager))
                throw new ComponentNotFoundException(
                    "No component LessonsInventoryManager has been found on the GameManager");
            LoadSpritesFromNames();
        }
        
        
        private void LoadSpritesFromNames()
        {
            foreach (string spriteName in lessonSpritesNames)
            {
                Sprite sprite = Resources.Load<Sprite>($"Sprites/Lessons/{spriteName}");
                if (sprite != null)
                {
                    _lessonsByLevel.Add(sprite);
                }
                else
                {
                    Debug.LogWarning($"Sprite '{spriteName}' not found in Resources/Sprites/Lessons/{spriteName}.");
                }
            }
        }

        public override void OnStartClient()
        {
            StartCoroutine(WaitForLocalPlayer());
        }

        private IEnumerator WaitForLocalPlayer()
        {
            // Wait until the localPlayer is set
            yield return new WaitUntil(() => NetworkClient.localPlayer is not null);
            Level = 1;
        }
        
        public void LevelUp()
        {
            Level++;
        }
        
    }
}