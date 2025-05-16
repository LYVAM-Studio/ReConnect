using System;

namespace Reconnect.Electronics.Breadboards.NetworkSync
{
    [Serializable]
    public class Uid : IEquatable<Uid>
    {
        public readonly uint Value;

        public Uid(uint value)
        {
            Value = value;
        }
        
        public bool Equals(Uid other)
            => other is not null && Value == other.Value;

        public override bool Equals(object obj)
            => obj is Uid other && Equals(other);

        public override int GetHashCode() => (int)Value;

        public override string ToString() => Value.ToString();

        public static implicit operator uint (Uid uid) => uid.Value;
        public static implicit operator Uid (uint uid) => new Uid(uid);
    }
}