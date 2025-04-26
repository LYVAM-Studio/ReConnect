using System;
using UnityEngine;
using Reconnect.Electronics.Breadboards;


namespace Reconnect.Electronics.Components
{
    public class WireScript : MonoBehaviour, IDipole
    {
        public Breadboard Breadboard { get; set; }

        public Vector2Int Pole1 { get; set; }
        public Vector2Int Pole2 { get; set; }

        private Outline _outline;

        private bool _isLocked = false;
        public bool IsLocked
        {
            get => _isLocked;
            set 
            {
                _isLocked = value;
                if (_isLocked) _outline.enabled = false;
            }
        }

        private void Awake()
        {
            _outline = GetComponent<Outline>();
            _outline.enabled = false;
        }

        private void OnMouseEnter()
        {
            if (!_isLocked) _outline.enabled = true;
        }

        private void OnMouseExit()
        {
            _outline.enabled = false;
        }

        private void OnMouseUpAsButton()
        {
            if (!_isLocked) Breadboard.DeleteWire(this);
        }

        // public static bool operator==(WireScript left, WireScript right) => left is not null && left.Equals(right);
        // public static bool operator!=(WireScript left, WireScript right) => !(left == right);
        // public override bool Equals(object obj) => obj is WireScript pole && Equals(pole) ;
        // private bool Equals(WireScript other) => Pole1 == other.Pole1 && Pole2 == other.Pole2;
        // public override int GetHashCode() => HashCode.Combine(Pole1, Pole2);
    }
}