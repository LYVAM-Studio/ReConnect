using System;
using UnityEngine;

namespace Reconnect.Electronics
{
    [Serializable]
    public sealed class Pole
    {
        public int H {get;}
        public int W {get;}
        public Pole(int h, int w)
        {
            H = h;
            W = w;
        }
            
        public static bool operator==(Pole left, Pole right) => left is not null && left.Equals(right);
        public static bool operator!=(Pole left, Pole right) => !(left == right);
        public override bool Equals(object obj) => obj is Pole pole && Equals(pole) ;
        private bool Equals(Pole other) => H == other.H && W == other.W;
        public override int GetHashCode() => HashCode.Combine(H, W);
        public override string ToString() => $"(h:{H},w:{W})";

        public static Pole PositionToPole(Vector2 position) => new((int)(position.y - 3.5f), (int)(position.x - 3.5f));
    }
}