using System;
using UnityEngine;

namespace Reconnect.Electronics.Components
{
    public class WireScript : MonoBehaviour
    {
        private bool _isInitialized;
        private Breadboard Breadboard { get; set; }
        public Pole Pole1 { get; private set; }
        public Pole Pole2 { get; private set; }

        private void OnMouseUpAsButton()
        {
            Breadboard.DeleteWire(this);
        }

        public void Init(Breadboard breadboard, Pole pole1, Pole pole2)
        {
            if (_isInitialized) throw new Exception("This Wire has already been initialized.");
            Breadboard = breadboard;
            Pole1 = pole1;
            Pole2 = pole2;
            _isInitialized = true;
        }

        // public static bool operator==(WireScript left, WireScript right) => left is not null && left.Equals(right);
        // public static bool operator!=(WireScript left, WireScript right) => !(left == right);
        // public override bool Equals(object obj) => obj is WireScript pole && Equals(pole) ;
        // private bool Equals(WireScript other) => Pole1 == other.Pole1 && Pole2 == other.Pole2;
        // public override int GetHashCode() => HashCode.Combine(Pole1, Pole2);
    }
}