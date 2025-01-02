using System;
using Mirror;
using Unity.Cinemachine;
using UnityEngine;
using Scene;
using Random = UnityEngine.Random;

namespace Player
{
    public class PlayerScript : NetworkBehaviour
    {
        public TextMesh playerNameText;
        public GameObject floatingInfo;

        private Material _playerMaterialClone;
        
        private SceneScript sceneScript;

        private CinemachineCamera _freeLookCamera;
        private Transform _lookAtObject;
        
        private int selectedWeaponLocal = 1;
        public GameObject[] weaponArray;

        [SyncVar(hook = nameof(OnNameChanged))]
        public string playerName;

        [SyncVar(hook = nameof(OnColorChanged))]
        public Color playerColor = Color.white;
        
        [SyncVar(hook = nameof(OnWeaponChanged))]
        public int activeWeaponSynced = 1;

        void Awake()
        {
            //allow all players to run this
            sceneScript = FindFirstObjectByType<SceneScript>(); // Changed the deprecated FindObjectOfType
            
            // get the FreeLookCamera game object
            _freeLookCamera = FindFirstObjectByType<CinemachineCamera>()??
                              throw new ArgumentException(
                                  "There is no freeLook Camera in this Scene");
            // get the object to look at
            _lookAtObject = gameObject.FindComponentsInChildrenWithName<Transform>("LookAt")[0] ??
                            throw new ArgumentException(
                                "There is no LookAt named gameObject in the children of the current GameObject");
            
            // disable all weapons
            foreach (var item in weaponArray)
                if (item != null)
                    item.SetActive(false);
        }

        [Command]
        public void CmdSendPlayerMessage()
        {
            if (sceneScript) 
                sceneScript.statusText = $"{playerName} says hello {Random.Range(10, 99)}";
        }

        [Command]
        public void CmdSetupPlayer(string _name, Color _col)
        {
            //player info sent to server, then server updates sync vars which handles it on all clients
            playerName = _name;
            playerColor = _col;
            sceneScript.statusText = $"{playerName} joined.";
        }

        private void OnNameChanged(string _Old, string _New)
        {
            playerNameText.text = playerName;
        }

        private void OnColorChanged(Color _Old, Color _New)
        {
            playerNameText.color = _New;
            _playerMaterialClone = new Material(GetComponent<Renderer>().material);
            _playerMaterialClone.color = _New;
            GetComponent<Renderer>().material = _playerMaterialClone;
        }

        public override void OnStartLocalPlayer()
        {
            //sceneScript.playerNetwork = this;
            /*
            Camera.main.transform.SetParent(transform);
            Camera.main.transform.localPosition = new Vector3(0, 0, 0);
            */
            
            _freeLookCamera.Follow = transform;
            _freeLookCamera.LookAt = _lookAtObject;
            
            floatingInfo.transform.localPosition = new Vector3(0, -0.3f, 0.6f);
            floatingInfo.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

            string name = "Player" + Random.Range(100, 999);
            Color color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
            CmdSetupPlayer(name, color);
        }

        void Update()
        {
            if (!isLocalPlayer)
            {
                // make non-local players run this
                floatingInfo.transform.LookAt(Camera.main.transform);
                return;
            }

            float moveX = Input.GetAxis("Horizontal") * Time.deltaTime * 110.0f;
            float moveZ = Input.GetAxis("Vertical") * Time.deltaTime * 4f;

            transform.Rotate(0, moveX, 0);
            transform.Translate(0, 0, moveZ);
            
            if (Input.GetButtonDown("Fire2")) //Fire2 is mouse 2nd click and left alt
            {
                selectedWeaponLocal += 1;

                if (selectedWeaponLocal > weaponArray.Length) 
                    selectedWeaponLocal = 1; 

                CmdChangeActiveWeapon(selectedWeaponLocal);
            }
        }
        
        void OnWeaponChanged(int _Old, int _New)
        {
            // disable old weapon
            // in range and not null
            if (0 < _Old && _Old < weaponArray.Length && weaponArray[_Old] != null)
                weaponArray[_Old].SetActive(false);
    
            // enable new weapon
            // in range and not null
            if (0 < _New && _New < weaponArray.Length && weaponArray[_New] != null)
                weaponArray[_New].SetActive(true);
        }

        [Command]
        public void CmdChangeActiveWeapon(int newIndex)
        {
            activeWeaponSynced = newIndex;
        }
    }
}
