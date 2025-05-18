using UnityEngine;

namespace Reconnect.Electronics.Breadboards
{
    public interface IDipole
    {
        public Breadboard Breadboard { get; }
        public Vector2Int Pole1 { get; set; }
        public Vector2Int Pole2 { get; set; }
        public bool IsLocked { get; set; }
        public Vector2Int[] GetPoles() => new[] { Pole1, Pole2 };
    }
}