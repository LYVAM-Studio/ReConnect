using Mirror;
using Reconnect.Menu.Lessons;
using Reconnect.Player;
using Reconnect.Utils;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Reconnect.CheatCodes
{
    public class CheatCodesManager : MonoBehaviour
    {
        private PlayerControls _controls;
        private TestLessons _testLessons;
        
        private void Awake()
        {
            if (!TryGetComponent(out _testLessons))
                throw new ComponentNotFoundException("No TestLessons component has been found on the CheatCodeManager");
            _controls = new PlayerControls();
            _controls.CheatCodes.KnockOut.performed += OnKnockOut;
            _controls.CheatCodes.PopulateLessons.performed += OnPopulateLessons;
        }
        
        private void OnEnable()
        {
            _controls.Enable();
        }

        private void OnDisable()
        {
            _controls.Disable();
        }

        private void OnDestroy()
        {
            _controls.CheatCodes.KnockOut.performed -= OnKnockOut;
            _controls.CheatCodes.PopulateLessons.performed -= OnPopulateLessons;
        }

        public void OnKnockOut(InputAction.CallbackContext ctx)
        {
            if (!NetworkClient.localPlayer.gameObject.TryGetComponent(out PlayerMovementsNetwork playerMovements))
                throw new ComponentNotFoundException(
                    "No PlayerMovementsNetwork found on the localPlayer gameObject");
            playerMovements.KnockOut();
        }
        
        public void OnPopulateLessons(InputAction.CallbackContext ctx)
        {
            _testLessons.PopulateLessons();
        }
    }
}
