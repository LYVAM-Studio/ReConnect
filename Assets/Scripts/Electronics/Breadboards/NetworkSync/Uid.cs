using System;

namespace Reconnect.Electronics.Breadboards.NetworkSync
{
    [Serializable]
    public class Uid : IEquatable<Uid>
    {
        public readonly int Value;

        public Uid(int value)
        {
            Value = value;
        }
        
        public bool Equals(Uid other)
            => other is not null && Value == other.Value;

        public override bool Equals(object obj)
            => obj is Uid other && Equals(other);

        public override int GetHashCode() => Value;

        public override string ToString() => Value.ToString();

        public static implicit operator int (Uid uid) => uid.Value;
        public static implicit operator Uid (int uid) => new Uid(uid);
    }
}