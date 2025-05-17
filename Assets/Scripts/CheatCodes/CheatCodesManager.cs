using System;
using Mirror;
using Reconnect.Menu;
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
        private PlayerGetter _localPlayerGetter;
        private void Awake()
        {
            if (!TryGetComponent(out _testLessons))
                throw new ComponentNotFoundException("No TestLessons component has been found on the CheatCodeManager");
            _controls = new PlayerControls();
            _controls.CheatCodes.KnockOut.performed += OnKnockOut;
            _controls.CheatCodes.CancelKnockOut.performed += OnCancelKnockOut;
            _controls.CheatCodes.PopulateLessons.performed += OnPopulateLessons;
            _controls.CheatCodes.SetLevel.performed += OnSetLevel;
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
            _controls.CheatCodes.CancelKnockOut.performed -= OnCancelKnockOut;
            _controls.CheatCodes.PopulateLessons.performed -= OnPopulateLessons;
            _controls.CheatCodes.SetLevel.performed -= OnSetLevel;
        }

        public void OnKnockOut(InputAction.CallbackContext ctx)
        {
            if (MenuManager.Instance.CurrentMenuState is not MenuState.None)
                return;
            
            if (!NetworkClient.localPlayer.gameObject.TryGetComponent(out PlayerMovementsNetwork playerMovements))
                throw new ComponentNotFoundException(
                    "No PlayerMovementsNetwork found on the localPlayer gameObject");
            
            MenuManager.Instance.SetKnockOutReason("This knock-out is triggered by a cheat code");
            playerMovements.KnockOut();
        }
        
        public void OnPopulateLessons(InputAction.CallbackContext ctx)
        {
            _testLessons.PopulateLessons();
        }

        public void OnCancelKnockOut(InputAction.CallbackContext ctx)
        {
            if (MenuManager.Instance.CurrentMenuState is not MenuState.KnockOut)
                return;
            
            if (!NetworkClient.localPlayer.gameObject.TryGetComponent(out PlayerMovementsNetwork playerMovements))
                throw new ComponentNotFoundException(
                    "No PlayerMovementsNetwork found on the localPlayer gameObject");

            playerMovements.CancelKnockOut();
            MenuManager.Instance.BackToPreviousMenu();
        }
        
        void OnSetLevel(InputAction.CallbackContext ctx)
        {
            int levelRaw = Mathf.RoundToInt(ctx.ReadValue<float>());
            if (levelRaw < 0)
                throw new InvalidCastException("Input level cheat code got negative value, cannot cast into uint");
            uint level = (uint)levelRaw;
            if (!NetworkClient.localPlayer.gameObject.TryGetComponent(out PlayerGetter playerGetter))
                throw new ComponentNotFoundException(
                    "No PlayerGetter found on the localPlayer gameObject");
            
            playerGetter.Network.CmdSetPlayersLevel(level);
        }
        
    }
}
