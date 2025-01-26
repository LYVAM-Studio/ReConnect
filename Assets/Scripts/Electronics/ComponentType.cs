using System;

namespace Reconnect.Electronics
{
    [Serializable]
    public enum ComponentType
    {
        Wire,
        Resistor
    }
    
    public static class ComponentTypeExtension
    {
        // Returns the supposed number of poles of this type of component
        public static int Poles(this ComponentType type) => type switch
        {
            ComponentType.Wire => 2,
            ComponentType.Resistor => 2,
            _ => throw new Exception($"Invalid electrical component type {(int)type}.")
        };
    }

}