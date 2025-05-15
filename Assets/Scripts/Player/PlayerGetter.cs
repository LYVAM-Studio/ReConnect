using Reconnect.Utils;
using UnityEngine;

namespace Reconnect.Player
{
    public class PlayerGetter : MonoBehaviour
    {
        public PlayerMovementsNetwork MovementsNetwork;
        public GameObject DummyModel;

        private void Awake()
        {
            if (!TryGetComponent(out MovementsNetwork))
                throw new ComponentNotFoundException("MovementNetwork not found");

            DummyModel = transform.GetChild(0).gameObject;
        }
    }
}