using System;
using Mirror;
using Reconnect.Utils;

namespace Reconnect.Electronics.Breadboards.NetworkSync
{
    public class ComponentSync : NetworkBehaviour
    {
        [SyncVar(hook = nameof(OnBreadboardSet))]
        public NetworkIdentity breadboardNetIdentity;
        [NonSerialized] public NetworkIdentity NetworkIdentity;
        protected virtual void OnBreadboardSet(NetworkIdentity oldValue, NetworkIdentity newValue)
        {
            if (newValue != null)
            {
                transform.SetParent(newValue.transform, false);
            }
        }

        protected void Awake()
        {
            if (!TryGetComponent(out NetworkIdentity))
                throw new ComponentNotFoundException("No network identity component has been found on this WireScript");
        }

        protected virtual void Start()
        {
            if (breadboardNetIdentity != null)
            {
                transform.SetParent(breadboardNetIdentity.transform, false);
            }
        }
    }
}