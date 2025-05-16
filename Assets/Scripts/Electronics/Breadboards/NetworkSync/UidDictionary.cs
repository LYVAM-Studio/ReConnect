using System;
using System.Collections.Generic;

namespace Reconnect.Electronics.Breadboards.NetworkSync
{
    public static class UidDictionary
    {
        private static readonly Dictionary<Uid, object> Dictionary = new();
        private static uint _nextId = 0;

        public static Uid Add(object item)
        {
            Uid id = new Uid(_nextId);
            _nextId++;
            Dictionary[id] = item;
            return id;
        }

        public static T Get<T>(Uid id)
        {
            if (!Dictionary.TryGetValue(id, out var obj))
                throw new KeyNotFoundException($"Item with ID {id} does not exist");
            if (!(obj is T tValue))
                throw new InvalidCastException($"Item with ID {id} is not of type {typeof(T).Name}.");
            return tValue;
        }

        public static bool Remove(Uid id)
        {
            return Dictionary.Remove(id);
        }
    }
}