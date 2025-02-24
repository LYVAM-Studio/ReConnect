using System;
using UnityEngine;

namespace Reconnect.Electronics
{
    [Serializable]
    public sealed class Pole
    {
        public Pole(int h, int w)
        {
            H = h;
            W = w;
        }

        public int H { get; }
        public int W { get; }

        public static bool operator ==(Pole left, Pole right)
        {
            return left is not null && left.Equals(right);
        }

        public static bool operator !=(Pole left, Pole right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            return obj is Pole pole && Equals(pole);
        }

        private bool Equals(Pole other)
        {
            return H == other.H && W == other.W;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(H, W);
        }

        public override string ToString()
        {
            return $"(h:{H},w:{W})";
        }

        public static Pole PositionToPole(Vector2 position)
        {
            return new Pole((int)(-position.y + 3.5f), (int)(position.x + 3.5f));
        }
        
        public static Vector3 PoleToPosition(Pole pole, float zPosition)
        {
            return new Vector3((pole.W - 3.5f),(-pole.H + 3.5f),zPosition);
        }
    }
}