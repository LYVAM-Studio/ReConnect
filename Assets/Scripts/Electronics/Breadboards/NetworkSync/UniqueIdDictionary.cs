using System;
using System.Collections.Generic;

namespace Reconnect.Electronics.Breadboards.NetworkSync
{
    public class UniqueIdDictionary
    {
        private static UniqueIdDictionary _instance;

        public static UniqueIdDictionary Instance
        {
            get { return _instance ??= new UniqueIdDictionary(); }
        }

        private readonly Dictionary<int, object> _dictionary = new();
        private int _nextId;

        private UniqueIdDictionary()
        {
            if (_instance is not null)
                throw new Exception("UniqueIdDictionary has already been instantiated");
        }
        
        public int Add(object item)
        {
            int id = _nextId++;
            _dictionary[id] = item;
            return id;
        }

        public T Get<T>(int id)
        {
            if (!_dictionary.TryGetValue(id, out var obj))
                throw new KeyNotFoundException($"Item with ID {id} does not exist");
            if (!(obj is T tValue))
                throw new InvalidCastException($"Item with ID {id} is not of type {typeof(T).Name}.");
            return tValue;
        }

        public bool Remove(int id)
        {
            return _dictionary.Remove(id);
        }

        public Dictionary<int, object> GetAll()
        {
            return _dictionary;
        }
    }
}