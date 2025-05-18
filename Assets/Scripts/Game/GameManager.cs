using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using Reconnect.Menu;
using Reconnect.Menu.Lessons;
using Reconnect.Player;
using Reconnect.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

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

            MenuManager.Instance.hudMenu.GetComponentInChildren<TMP_Text>().text = $"Level : {newLevel}";
            MenuManager.Instance.LockPlayer();
            // TODO : display indication on the new level
            int lessonIndex = (int)newLevel - 1;
            MenuManager.Instance.newLessonController.LoadImage(_lessonsByLevel[lessonIndex]);
            MenuManager.Instance.newLessonController.SetTextToLevel(newLevel);
            FreeLookCamera.InputAxisController.enabled = false;
            MenuManager.Instance.SetMenuTo(MenuState.NewLesson, CursorState.Shown);
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
            // lessons to load in the order of the levels
            string[] lessonSprites =
            {
                "Reading a circuit diagram",
                "Intensity in a circuit",
                "Tension in a circuit",
                "Resistance in a circuit",
                "Resistance color code"
            };

            foreach (string spriteName in lessonSprites)
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