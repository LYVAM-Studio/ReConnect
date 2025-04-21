using System;
using UnityEngine;
using Reconnect.Electronics.Breadboards;


namespace Reconnect.Electronics.Components
{
    public class WireScript : MonoBehaviour
    {
        private bool _isInitialized;
        private Breadboard Breadboard { get; set; }
        public Point Pole1 { get; private set; }
        public Point Pole2 { get; private set; }

        private Outline _outline;

        private bool _isLocked = false;
        public bool IsLocked
        {
            get => _isLocked;
            set
            {
                _isLocked = value;
                _outline.OutlineColor = _isLocked ? Color.red : Color.white;
            }
        }

        private void Awake()
        {
            _outline = GetComponent<Outline>();
            _outline.enabled = false;
            _outline.OutlineColor = Color.white;
        }

        private void OnMouseEnter()
        {
            _outline.OutlineColor = _isLocked ? Color.red : Color.white;
            _outline.enabled = true;
        }

        private void OnMouseExit()
        {
            _outline.enabled = false;
        }

        private void OnMouseUpAsButton()
        {
            if (!_isLocked) Breadboard.DeleteWire(this);
        }

        public void Init(Breadboard breadboard, Point pole1, Point pole2, bool isLocked = false)
        {
            if (_isInitialized) throw new Exception("This Wire has already been initialized.");
            Breadboard = breadboard;
            Pole1 = pole1;
            Pole2 = pole2;
            _isInitialized = true;
            IsLocked = isLocked;
        }

        // public static bool operator==(WireScript left, WireScript right) => left is not null && left.Equals(right);
        // public static bool operator!=(WireScript left, WireScript right) => !(left == right);
        // public override bool Equals(object obj) => obj is WireScript pole && Equals(pole) ;
        // private bool Equals(WireScript other) => Pole1 == other.Pole1 && Pole2 == other.Pole2;
        // public override int GetHashCode() => HashCode.Combine(Pole1, Pole2);
    }
}