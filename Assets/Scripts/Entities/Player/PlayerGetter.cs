using System;
using Mirror;
using Reconnect.Utils;
using UnityEngine;

namespace Reconnect.Player
{
    public class PlayerGetter : NetworkBehaviour
    {
        [NonSerialized] public PlayerMovementsNetwork Movements;
        [NonSerialized] public PlayerLevel Level;
        [NonSerialized] public PlayerNetwork Network;
        [NonSerialized] public GameObject DummyModel;

        public override void OnStartClient()
        {
            if (!TryGetComponent(out Movements))
                throw new ComponentNotFoundException("MovementNetwork not found");
            if (!TryGetComponent(out Network))
                throw new ComponentNotFoundException("PlayerNetwork not found");
            if (!TryGetComponent(out Level))
                throw new ComponentNotFoundException("PlayerLevel not found");

            DummyModel = netIdentity.transform.GetChild(0).gameObject;
        }
    }
}