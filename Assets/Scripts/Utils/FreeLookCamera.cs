using Unity.Cinemachine;
using UnityEngine;

namespace Reconnect.Utils
{
    public static class FreeLookCamera
    {
        private static GameObject _gameObject;
        private static CinemachineCamera _virtualCamera;
        private static CinemachineInputAxisController _inputAxisController;

        public static GameObject GameObject
        {
            get
            {
                if (_gameObject is null)
                {
                    _gameObject = GameObject.FindGameObjectWithTag("freeLookCamera");
                    if (_gameObject is null)
                        throw new GameObjectNotFoundException("No game object with tag freeLookCamera has been found in the scene.");
                }

                return _gameObject;
            }
        }

        public static CinemachineCamera VirtualCamera
        {
            get
            {
                if (_virtualCamera is null && !GameObject.TryGetComponent(out _virtualCamera))
                    throw new ComponentNotFoundException(
                        "No CinemachineCamera component has been found on the FreeLookCamera.");
                
                return _virtualCamera;
            }
        }
        
        public static CinemachineInputAxisController InputAxisController
        {
            get
            {
                if (_inputAxisController is null && !GameObject.TryGetComponent(out _inputAxisController))
                    throw new ComponentNotFoundException(
                        "No CinemachineInputAxisController component has been found on the FreeLookCamera.");
                
                return _inputAxisController;
            }
        }
    }
}