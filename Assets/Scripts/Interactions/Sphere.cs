using UnityEngine;

namespace Reconnect.Interactions
{
    public class Sphere : MonoBehaviour, IInteractable
    {
        // public string destinationScene = "SphereScene";
        private bool _isBig;
        private Transform _transform;

        public void Start()
        {
            _isBig = false;
            _transform = GetComponent<Transform>();
        }
        
        public void Interact()
        {
            if (_isBig)
                _transform.localScale = new Vector3(1, 1, 1);
            else
                _transform.localScale = new Vector3(2, 2, 2);
        }

        public bool CanInteract()
        {
            return true;
        }
    }

}
