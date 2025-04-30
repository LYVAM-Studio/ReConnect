using Reconnect.Electronics.CircuitLoading;
using UnityEngine;

namespace Reconnect.Electronics.UI
{
    public class LoadLevelMenu : MonoBehaviour
    {
        private BreadboardUI _breadboardUI;

        public string CircuitName;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _breadboardUI = GetComponentInParent<BreadboardUI>();
        }

        public void LoadCircuitLevel()
        {
            Loader.LoadCircuit(_breadboardUI.Breadboard, CircuitName);
        }
    }
}