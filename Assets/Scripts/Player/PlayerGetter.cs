using Mirror;
using Reconnect.Utils;
using UnityEngine;

namespace Reconnect.Player
{
    public class PlayerGetter : NetworkBehaviour
    {
        public PlayerMovementsNetwork MovementsNetwork;
        public GameObject DummyModel;

        public override void OnStartClient()
        {
            if (!TryGetComponent(out MovementsNetwork))
                throw new ComponentNotFoundException("MovementNetwork not found");

            DummyModel = netIdentity.transform.GetChild(0).gameObject;
        }
    }
}