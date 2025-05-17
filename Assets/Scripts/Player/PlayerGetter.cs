using Mirror;
using Reconnect.Utils;
using UnityEngine;

namespace Reconnect.Player
{
    public class PlayerGetter : NetworkBehaviour
    {
        public PlayerMovementsNetwork MovementsNetwork;
        public PlayerNetwork PlayerNetwork;
        public GameObject DummyModel;

        public override void OnStartClient()
        {
            if (!TryGetComponent(out MovementsNetwork))
                throw new ComponentNotFoundException("MovementNetwork not found");
            if (!TryGetComponent(out PlayerNetwork))
                throw new ComponentNotFoundException("PlayerNetwork not found");

            DummyModel = netIdentity.transform.GetChild(0).gameObject;
        }
    }
}