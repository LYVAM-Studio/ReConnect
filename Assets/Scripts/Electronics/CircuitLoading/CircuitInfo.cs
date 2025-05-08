using UnityEngine;

namespace Reconnect.Electronics.CircuitLoading
{
    public struct CircuitInfo
    {
        public string Title;
        public float InputTension;
        public float InputIntensity;
        public float TargetTension;
        public Vector2Int InputPoint;
        public Vector2Int OutputPoint;
    }
}