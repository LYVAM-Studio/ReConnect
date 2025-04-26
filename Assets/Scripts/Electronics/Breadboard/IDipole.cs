using System;
using UnityEngine;

namespace Reconnect.Electronics.Breadboards
{
    public interface IDipole
    {
        public Breadboard Breadboard { get; set; }
        public Vector2Int Pole1 { get;set; }
        public Vector2Int Pole2 { get;set; } 
    }
}