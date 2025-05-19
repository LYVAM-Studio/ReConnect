using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using Reconnect.Menu;
using Reconnect.Menu.Lessons;
using Reconnect.Player;
using Reconnect.Utils;
using UnityEngine;
using UnityEngine.Serialization;

namespace Reconnect.Game
{
    public class GameManager : NetworkBehaviour
    {
        [NonSerialized] public static GameManager Instance;
        
        public static uint Level;

        private static readonly List<Sprite> LessonsByLevel = new();

        private static LessonsInventoryManager _lessonsInventoryManager;
        
        // lessons to load in the order of the levels
        private static readonly string[] LessonSpritesNames =
        {
            "Reading a circuit diagram",
            "Resistance in a circuit",
            "Tension in a circuit",
            "Intensity in a circuit",
            "Resistance color code"
        };
        [Header("Level triggered objects")]
        [SerializeField] private Transform lights;
        private static Transform _staticLights;
        [SerializeField] private Transform doors;
        private static Transform _staticDoors;
        [SerializeField] private Transform fans;
        private static Transform _staticFans;
        [SerializeField] private Transform pumps;
        private static Transform _staticPumps;

        [Header("Level triggers datas")] 
        [SerializeField] private float doorTargetYPosotion;
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
            Instance = this;
            Level = 1;
            _staticPumps = pumps;
            _staticFans = fans;
            _staticDoors = doors;
            _staticLights = lights;
        }

        private IEnumerator WaitForLocalPlayer()
        {
            // Wait until the localPlayer is set
            yield return new WaitUntil(() => NetworkClient.localPlayer is not null);
            if (!NetworkClient.localPlayer.TryGetComponent(out PlayerGetter playerGetter))
                throw new ComponentNotFoundException("No PlayerGetter found on the local player");
            playerGetter.Network.CmdGetPlayerLevel();
        }

        public void LevelTrigger(uint level, PlayerNetwork playerNetwork)
        {
            switch (level)
            {
                case 2 :
                    TriggerLevel2(playerNetwork);
                    break;
                case 3 :
                    TriggerLevel3();
                    break;
                case 4 :
                    TriggerLevel4(playerNetwork);
                    break;
                case 5 :
                    TriggerLevel5(playerNetwork);
                    break;
            }
        }

        private void TriggerLevel2(PlayerNetwork playerNetwork)
        {
            foreach (Transform triggeredLight in lights)
            {
                Debug.Log($"lamp {triggeredLight.name} turned on");
                if(!triggeredLight.TryGetComponent(out NetworkIdentity lightIdentity))
                    Debug.LogException(new ComponentNotFoundException("No network identity on the target light"));
                playerNetwork.RpcSetEnabledLight(lightIdentity, true);
            }
        }

        private IEnumerator OpenDoor(Transform door)
        {
            while (doorTargetYPosotion - door.localPosition.y > 0.01f)
            {
                Debug.Log("door are moving...");
                door.localPosition += new Vector3(0, Time.deltaTime * 2f, 0);
                yield return null;
            }
            Debug.Log("door are moved.");
            door.localPosition = new Vector3(door.localPosition.x, doorTargetYPosotion, door.localPosition.z);
        }
        
        private void TriggerLevel3()
        {
            foreach (Transform door in doors)
            {
                StartCoroutine(OpenDoor(door));
            }
        }
        
        private void TriggerLevel4(PlayerNetwork playerNetwork)
        {
            foreach (Transform fan in fans)
            {
                if(!fan.TryGetComponent(out NetworkIdentity fanIdentity))
                    Debug.LogException(new ComponentNotFoundException("No network identity on the target fan"));
                playerNetwork.RpcSetEnabledAnimation(fanIdentity, true);
                playerNetwork.RpcSetEnabledAudio(fanIdentity, true);
            }
        }
        
        private void TriggerLevel5(PlayerNetwork playerNetwork)
        {
            foreach (Transform pump in pumps)
            {
                if(!pump.TryGetComponent(out NetworkIdentity pumpIdentity))
                    Debug.LogException(new ComponentNotFoundException("No network identity on the target pump"));
                playerNetwork.RpcSetEnabledAnimation(pumpIdentity, true);
            }
        }
        
    }
}