using System;
using UnityEngine;

namespace Reconnect.Electronics.Breadboards
{
    [Serializable]
    public sealed class Point
    {
        public Point(int h, int w)
        {
            H = h;
            W = w;
        }

        public int H { get; }
        public int W { get; }

        public Point Shift(int deltaH, int deltaW) => new Point(H + deltaH, W + deltaW);
        public Point ShiftH(int deltaH) => Shift(deltaH, 0);
        public Point ShiftW(int deltaW) => Shift(0, deltaW);

        public static bool operator ==(Point left, Point right)
        {
            return left is not null && left.Equals(right);
        }

        public static bool operator !=(Point left, Point right)
        {
            return !(left == right);
        }

        public static Point operator +(Point p1, Point p2) => new Point(p1.H + p2.H, p1.W + p2.W);
        
        public static Point operator -(Point p1, Point p2) => new Point(p1.H - p2.H, p1.W - p2.W);
        
        public static Point operator -(Point p) => new Point(-p.H, -p.W);

        public override bool Equals(object obj)
        {
            return obj is Point pole && Equals(pole);
        }

        private bool Equals(Point other)
        {
            return H == other.H && W == other.W;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(H, W);
        }

        public override string ToString()
        {
            return $"(h: {H},w: {W})";
        }

        public static Point VectorToPoint(Vector2 position)
        {
            return new Point((int)(-position.y + 3.5f), (int)(position.x + 3.5f));
        }
        
        public static Vector3 PointToVector(Point point, float zPosition)
        {
            return new Vector3((point.W - 3.5f),(-point.H + 3.5f),zPosition);
        }
        
        public static explicit operator Vector2(Point p)
        {
            return new Vector2(p.W, p.H);
        }

    }
}