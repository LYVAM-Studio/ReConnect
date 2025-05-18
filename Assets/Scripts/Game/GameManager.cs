using System.Collections;
using System.Collections.Generic;
using Mirror;
using Reconnect.Menu;
using Reconnect.Menu.Lessons;
using Reconnect.Player;
using Reconnect.Utils;
using UnityEngine;

namespace Reconnect.Game
{
    public class GameManager : NetworkBehaviour
    {
        public static uint Level;

        private static readonly List<Sprite> LessonsByLevel = new();

        private static LessonsInventoryManager _lessonsInventoryManager;
        
        // lessons to load in the order of the levels
        private static readonly string[] LessonSpritesNames =
        {
            "Reading a circuit diagram",
            "Intensity in a circuit",
            "Tension in a circuit",
            "Resistance in a circuit",
            "Resistance color code"
        };
        
        public static void OnLevelChange(uint oldLevel, uint newLevel)
        {
            if (newLevel == 0)
            {
                Debug.LogError("The new level is null and it should not be!");
                return;
            }
            // change the HUD level text
            MenuManager.Instance.SetLevel(newLevel);
            // show the canva of the mission brief for this level
            MenuManager.Instance.SetMenuToMissionBrief(newLevel);
            int lessonIndex = (int)newLevel - 1;
            // show the menu of the new lesson with the image of the lesson
            MenuManager.Instance.SetMenuToNewLesson(newLevel, LessonsByLevel[lessonIndex]);
            for (uint level = oldLevel + 1; level <= newLevel; level++)
            {
                _lessonsInventoryManager.AddItem(LessonsByLevel[(int)level - 1].name, LessonsByLevel[(int)level - 1]);
            }
        }
        
        private void Awake()
        {
            if (!TryGetComponent(out _lessonsInventoryManager))
                throw new ComponentNotFoundException(
                    "No component LessonsInventoryManager has been found on the GameManager");
            LoadSpritesFromNames();
        }
        
        
        private static void LoadSpritesFromNames()
        {
            foreach (string spriteName in LessonSpritesNames)
            {
                Sprite sprite = Resources.Load<Sprite>($"Sprites/Lessons/{spriteName}");
                if (sprite != null)
                {
                    LessonsByLevel.Add(sprite);
                }
                else
                {
                    Debug.LogWarning($"Sprite '{spriteName}' not found in Resources/Sprites/Lessons/{spriteName}.");
                }
            }
        }

        public override void OnStartClient()
        {
            if(isServer)
                OnLevelChange(0,1);
            else
            {
                StartCoroutine(WaitForLocalPlayer());
            }
        }

        public override void OnStartServer()
        {
            Level = 1;
            if (isClient)
            {
                OnLevelChange(0,1);
            }
        }

        private IEnumerator WaitForLocalPlayer()
        {
            // Wait until the localPlayer is set
            yield return new WaitUntil(() => NetworkClient.localPlayer is not null);
            if (!NetworkClient.localPlayer.TryGetComponent(out PlayerGetter playerGetter))
                throw new ComponentNotFoundException("No PlayerGetter found on the local player");
            playerGetter.Network.CmdGetPlayerLevel();
        }
        
        public static void LevelUp()
        {
            Level++;
        }
        
    }
}