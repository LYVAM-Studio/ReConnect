using System;
using UnityEngine;

namespace Reconnect.Electronics.Breadboards
{
    public class Liason
    {
        public Vector2 P1 { get; }
        public Vector2 P2 { get; }

        public Liason(Vector2 p1, Vector2 p2)
        {
            P1 = p1; P2 = p2;
        }

        public static bool operator ==(Liason l1, Liason l2)
        {
            return l1.P1 == l2.P1 && l1.P2 == l2.P2 || l1.P1 == l2.P2 && l1.P2 == l2.P1;
        }

        public static bool operator !=(Liason l1, Liason l2)
        {
            return !(l1 == l2);
        }
        
        protected bool Equals(Liason other)
        {
            return Equals(P1, other.P1) && Equals(P2, other.P2);
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Liason)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(P1, P2);
        }
    }
}